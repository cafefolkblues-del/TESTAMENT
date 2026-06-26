using TMPro;
using UnityEngine;

// 무전기 위젯 — 우상단 상시 표시(무전기 그래픽 placeholder), 메시지는 RadioFeed로 들어와 잠깐 떴다 페이드.
// UI 기능 분리: RadioFeed 채널만 구독, 다른 위젯과 결합 없음. 동료 초상화/대사는 추후 이 위젯에 슬롯 추가.
public class RadioWidget : MonoBehaviour
{
    [SerializeField] CanvasGroup _messageGroup;   // 메시지 라인만 페이드 (무전기 본체는 상시)
    [SerializeField] TMP_Text    _message;
    [SerializeField] float       _showDuration = 3f;
    [SerializeField] float       _fadeSpeed    = 3f;

    float _timer;

    void OnEnable()  => RadioFeed.OnMessage += Show;
    void OnDisable() => RadioFeed.OnMessage -= Show;   // 씬 전환 누수 방지

    void Show(string msg)
    {
        _message.text     = msg;
        _timer            = _showDuration;
        _messageGroup.alpha = 1f;
    }

    void Update()
    {
        if (_timer > 0f) _timer -= Time.deltaTime;
        else _messageGroup.alpha = Mathf.MoveTowards(_messageGroup.alpha, 0f, _fadeSpeed * Time.deltaTime);
    }
}
