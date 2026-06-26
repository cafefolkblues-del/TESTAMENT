using System.Collections.Generic;
using UnityEngine;

public enum DialoguePriority { Ambient, Trigger, Critical }

// 대사 매니저 (씬 1개). 세 노선(앰비언트/사망/거대형)이 DialogueRequest로 우선순위 큐에 enqueue,
// 소비기가 1개씩 최고우선 표시. 연속발화 금지=직전 발화자 하드 제외.
public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public struct DeathTrigger
    {
        public CharacterData victim;
        public CharacterData reactor;
        [TextArea] public string line;
    }

    struct Request
    {
        public CharacterData    speaker;
        public string           line;
        public DialoguePriority priority;
    }

    [SerializeField] GameState _gameState;
    [SerializeField] Vector2   _ambientInterval = new Vector2(8f, 15f);
    [SerializeField] float     _displayDuration = 3f;
    [SerializeField] int       _queueCap        = 2;
    [SerializeField] DeathTrigger[] _deathTriggers;   // 확정: 릴 사망 → 루시

    ContextTracker _context;
    readonly List<Request> _queue = new List<Request>();
    float          _ambientTimer;
    float          _displayTimer;
    CharacterData  _lastSpeaker;
    readonly Dictionary<DialogueProfile, int> _lastLineIndex = new Dictionary<DialogueProfile, int>();

    void OnEnable()
    {
        _context = new ContextTracker(_gameState);
        CompanionHealth.OnAnyDeath      += OnCompanionDeath;
        EnemySpawner.OnGiantApproaching += OnGiantApproaching;
        ResetAmbientTimer();
    }

    void OnDisable()
    {
        if (_context != null) _context.Dispose();
        CompanionHealth.OnAnyDeath      -= OnCompanionDeath;
        EnemySpawner.OnGiantApproaching -= OnGiantApproaching;
    }

    void Update()
    {
        _ambientTimer -= Time.deltaTime;
        if (_ambientTimer <= 0f) { ResetAmbientTimer(); TryGenerateAmbient(); }

        if (_displayTimer > 0f) { _displayTimer -= Time.deltaTime; return; }
        if (_queue.Count > 0) Consume();
    }

    void ResetAmbientTimer() => _ambientTimer = Random.Range(_ambientInterval.x, _ambientInterval.y);

    // ── 큐 ──
    void Enqueue(CharacterData speaker, string line, DialoguePriority priority)
    {
        var req = new Request { speaker = speaker, line = line, priority = priority };
        if (priority == DialoguePriority.Critical) { _queue.Clear(); _queue.Add(req); return; }
        if (_queue.Count < _queueCap) { _queue.Add(req); return; }

        int low = 0;
        for (int i = 1; i < _queue.Count; i++) if (_queue[i].priority < _queue[low].priority) low = i;
        if (priority > _queue[low].priority) { _queue.RemoveAt(low); _queue.Add(req); }
    }

    void Consume()
    {
        int best = 0;
        for (int i = 1; i < _queue.Count; i++) if (_queue[i].priority > _queue[best].priority) best = i;
        var req = _queue[best];
        _queue.RemoveAt(best);

        _lastSpeaker  = req.speaker;
        _displayTimer = _displayDuration;
        RadioFeed.Post(req.line, req.speaker);
    }

    // ── 앰비언트 ──
    void TryGenerateAmbient()
    {
        if (_queue.Count >= _queueCap) return;
        var ctx     = _context.Evaluate();
        var speaker = SelectAmbientSpeaker(ctx);
        if (speaker == null) return;
        var line = PickLine(speaker.dialogue, ctx);
        if (line == null) return;
        Enqueue(speaker, line, DialoguePriority.Ambient);
    }

    // 직전 발화자 제외 우선 추첨 → 후보 없으면 제외 풀고 재시도 (연속발화 금지)
    CharacterData SelectAmbientSpeaker(DialogueContext ctx)
    {
        var s = AmbientRoll(ctx, true);
        return s != null ? s : AmbientRoll(ctx, false);
    }

    CharacterData AmbientRoll(DialogueContext ctx, bool excludeLast)
    {
        float total = 0f;
        foreach (var cd in _gameState.companions)
        {
            if (!Speakable(cd) || (excludeLast && cd.character == _lastSpeaker)) continue;
            total += AmbientWeight(cd.character, ctx);
        }
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total), cum = 0f;
        foreach (var cd in _gameState.companions)
        {
            if (!Speakable(cd) || (excludeLast && cd.character == _lastSpeaker)) continue;
            cum += AmbientWeight(cd.character, ctx);
            if (roll <= cum) return cd.character;
        }
        return null;
    }

    // 점수 = 기본빈도 × 상황배율 × 릴윈도우 (직전 발화자는 추첨에서 아예 제외 — 페널티 대신 금지)
    float AmbientWeight(CharacterData c, DialogueContext ctx)
    {
        var p = c.dialogue;
        return p.baseFrequency * p.MultFor(ctx) * ReactionBoost(c);
    }

    // 릴 리액션 윈도우 — 직전 발화자별 추가배율 (릴 프로필만 채움)
    float ReactionBoost(CharacterData c)
    {
        if (_lastSpeaker == null || c.dialogue.reactionBoosts == null) return 1f;
        foreach (var rb in c.dialogue.reactionBoosts)
            if (rb.speaker == _lastSpeaker) return rb.mult;
        return 1f;
    }

    string PickLine(DialogueProfile p, DialogueContext ctx)
    {
        var pool = p.LinesFor(ctx);
        if (pool == null || pool.Length == 0) return null;
        if (pool.Length == 1) return pool[0];

        int last = _lastLineIndex.TryGetValue(p, out var li) ? li : -1;
        int idx;
        do { idx = Random.Range(0, pool.Length); } while (idx == last);
        _lastLineIndex[p] = idx;
        return pool[idx];
    }

    // ── 트리거: 동료 사망 (Critical) ──
    void OnCompanionDeath(CompanionData dead)
    {
        // 1) 확정 반응 (릴 → 루시 등)
        foreach (var dt in _deathTriggers)
        {
            if (dt.victim != dead.character || !IsAlive(dt.reactor)) continue;
            Enqueue(dt.reactor, dt.line, DialoguePriority.Critical);
            return;
        }
        // 2) fallback — 생존 동료(사망자 제외)가 일반 사망반응. 모든 죽음이 체감되도록.
        var reactor = SelectDeathReactor(dead.character);
        if (reactor != null)
        {
            var lines = reactor.dialogue.deathReactionLines;
            Enqueue(reactor, lines[Random.Range(0, lines.Length)], DialoguePriority.Critical);
        }
    }

    CharacterData SelectDeathReactor(CharacterData dead)
    {
        float total = 0f;
        foreach (var cd in _gameState.companions)
            if (CanReactDeath(cd, dead)) total += cd.character.dialogue.baseFrequency;
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total), cum = 0f;
        foreach (var cd in _gameState.companions)
        {
            if (!CanReactDeath(cd, dead)) continue;
            cum += cd.character.dialogue.baseFrequency;
            if (roll <= cum) return cd.character;
        }
        return null;
    }

    bool CanReactDeath(CompanionData cd, CharacterData dead)
    {
        return Speakable(cd) && cd.character != dead
            && cd.character.dialogue.deathReactionLines != null
            && cd.character.dialogue.deathReactionLines.Length > 0;
    }

    // ── 트리거: 거대형 예고 (Trigger) — 직전 발화자 제외 우선 ──
    void OnGiantApproaching()
    {
        var speaker = GiantRoll(true);
        if (speaker == null) speaker = GiantRoll(false);
        if (speaker == null) return;   // 전멸/라인 없음 → 침묵 (GDD)
        var lines = speaker.dialogue.giantWarningLines;
        Enqueue(speaker, lines[Random.Range(0, lines.Length)], DialoguePriority.Trigger);
    }

    CharacterData GiantRoll(bool excludeLast)
    {
        float total = 0f;
        foreach (var cd in _gameState.companions)
        {
            if (!HasWarning(cd) || (excludeLast && cd.character == _lastSpeaker)) continue;
            total += cd.character.dialogue.baseFrequency;
        }
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total), cum = 0f;
        foreach (var cd in _gameState.companions)
        {
            if (!HasWarning(cd) || (excludeLast && cd.character == _lastSpeaker)) continue;
            cum += cd.character.dialogue.baseFrequency;
            if (roll <= cum) return cd.character;
        }
        return null;
    }

    // ── 공통 ──
    bool Speakable(CompanionData cd) => cd.isAlive && cd.character != null && cd.character.dialogue != null;

    bool HasWarning(CompanionData cd) =>
        Speakable(cd) && cd.character.dialogue.giantWarningLines != null && cd.character.dialogue.giantWarningLines.Length > 0;

    bool IsAlive(CharacterData c)
    {
        foreach (var cd in _gameState.companions) if (cd.character == c) return cd.isAlive;
        return false;
    }
}
