using System.Collections;
using TMPro;
using UnityEngine;

// 사망 흐름의 분기/시퀀스/입력 처리는 GameSceneManager로 이관됨.
// 이 클래스는 결과 패널의 표시·숨김만 책임 (단일 책임).
public class ResultUI : MonoBehaviour
{
    // TMP_Text 사용 중 — 아트 에셋 준비 시 Image 기반 UI로 교체 가능
    [SerializeField] CanvasGroup _panel;
    [SerializeField] TMP_Text    _scoreText;
    [SerializeField] TMP_Text    _coinText;
    [SerializeField] float       _fadeInDuration = 0.3f;

    void Awake()
    {
        // 사망 전까지 비표시 보장 — 인스펙터 의존 제거 (방어적 초기화)
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;
    }

    // IEnumerator 반환 이유 — 호출자(GameSceneManager)가 yield return으로 페이드인 완료 대기.
    // 점수 포맷은 ScoreFormat에 위임 — 자릿수/콤마 등 표시 규칙 변경 시 ResultUI 안 건드림
    public IEnumerator Show(float score, int coin)
    {
        _scoreText.text = ScoreFormat.Display(score);
        _coinText.text  = coin.ToString();
        _panel.interactable   = true;
        _panel.blocksRaycasts = true;
        yield return _panel.Fade(0f, 1f, _fadeInDuration);
    }

    // 부활 성공 시 외부에서 호출 — 패널 닫기 (페이드 아웃은 현재 기획상 없음)
    public void Hide()
    {
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;
    }
}
