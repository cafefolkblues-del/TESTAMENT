using System.Collections;
using TMPro;
using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] CharacterRoster _roster;            // 마스터 데이터 — 캐릭터 5종 단일 진실
    [SerializeField] CharacterPanel _panelPrefab;        // 동적 생성용 prefab
    [SerializeField] Transform _panelContainer;          // HorizontalLayoutGroup 부착 부모
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] float blinkInterval        = 0.1f;
    [SerializeField] float fadeDuration         = 0.8f;
    [SerializeField] float titleCharInterval    = 0.05f;
    [SerializeField] float panelFadeInDuration  = 0.3f;  // 패널 본체 페이드 길이
    [SerializeField] float panelFadeInStagger   = 0.1f;  // 패널 간 시작 간격 (시차 등장)

    CharacterPanel[] _panels;

    void Start()
    {
        // 매 진입 시 SO 비움 — 이전 런 선택값이 살아 들어오는 사고 차단
        _selectedCharacter.Reset();

        _panels = SpawnPanels();
        StartCoroutine(IntroSequence());
    }

    // Roster 배열 순서대로 prefab 인스턴스화 — 자식 순서 비결정성 제거.
    // Instantiate(prefab, parent) — parent 직속 자식. LayoutGroup이 정렬 책임.
    CharacterPanel[] SpawnPanels()
    {
        var data = _roster.characters;
        var panels = new CharacterPanel[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var panel = Instantiate(_panelPrefab, _panelContainer);
            panel.Init(this, data[i]);
            panels[i] = panel;
        }
        return panels;
    }

    // 진입 시퀀스 — 타이틀 타이핑 끝나면 패널 시차 등장
    IEnumerator IntroSequence()
    {
        yield return AnimateTitle();
        yield return PanelsCascadeFadeIn();
    }

    IEnumerator AnimateTitle()
    {
        // maxVisibleCharacters 채택 이유 — TMP_Text의 문자 단위 점진 표시. Substring 갱신보다 가벼움
        _titleText.maxVisibleCharacters = 0;
        _titleText.ForceMeshUpdate();
        int total = _titleText.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            _titleText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(titleCharInterval);
        }
    }

    // 패널 시차 페이드인 — stagger 간격으로 FadeIn 시작, 마지막 페이드 끝나면 일괄 Unlock
    IEnumerator PanelsCascadeFadeIn()
    {
        foreach (var panel in _panels)
        {
            // StartCoroutine으로 시작 — yield return하지 않고 stagger만큼만 대기 후 다음 패널 시작
            StartCoroutine(panel.FadeIn(panelFadeInDuration));
            yield return new WaitForSeconds(panelFadeInStagger);
        }
        // 마지막 패널 페이드 완료까지 잔여시간 대기 (duration < stagger 케이스 가드)
        yield return new WaitForSeconds(Mathf.Max(0f, panelFadeInDuration - panelFadeInStagger));
        // 등장 완료 — 모든 패널 입력 활성
        foreach (var panel in _panels) panel.Unlock();
    }

    public void OnPanelSelected(CharacterPanel panel)
    {
        _selectedCharacter.character = panel.Data;
        // 선택 후 추가 입력 차단 — 다른 패널 클릭 방지
        foreach (var p in _panels) p.Lock();
        StartCoroutine(PlayTransition(panel));
    }

    IEnumerator PlayTransition(CharacterPanel panel)
    {
        // BlinkWhite도 IEnumerator 반환 — yield return으로 chain
        yield return panel.BlinkWhite(2, blinkInterval);
        // 페이드 4중 복붙 통합의 일부
        yield return _fadeOverlay.Fade(0f, 1f, fadeDuration);
        // 마법 문자열 nextSceneName 제거 → SceneFlow 단일 진입점
        SceneFlow.Load(SceneId.Game);
    }
}
