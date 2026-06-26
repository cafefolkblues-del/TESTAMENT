using UnityEngine;

// 전황 판정 — DialogueManager가 보유. 데이터 소스 전부 기존(GameState/Enemy/EnemyHealth.OnAnyKilled 재사용).
// Evaluate는 발화 타이머 틱(8~15s)에만 호출 → FindObjects 비용 무시 가능.
public class ContextTracker
{
    readonly GameState _gs;
    float _lastKillTime;

    public ContextTracker(GameState gs)
    {
        _gs = gs;
        EnemyHealth.OnAnyKilled += OnKill;
    }

    public void Dispose() => EnemyHealth.OnAnyKilled -= OnKill;

    void OnKill() => _lastKillTime = Time.time;

    public DialogueContext Evaluate()
    {
        int deadAlly = 0;
        foreach (var c in _gs.companions) if (!c.isAlive) deadAlly++;

        // Stress: 코인 바닥 + 동료 다수 전사
        if (_gs.coinCount <= 1 && deadAlly >= 2) return DialogueContext.Stress;

        // Lull: 적 없음 + 마지막 처치 후 5초 경과
        int enemies = Object.FindObjectsOfType<Enemy>().Length;
        if (enemies == 0 && Time.time - _lastKillTime > 5f) return DialogueContext.Lull;

        return DialogueContext.Combat;
    }
}
