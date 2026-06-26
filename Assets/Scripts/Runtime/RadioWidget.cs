using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 무전기 위젯 — 우상단 상시 표시, 메시지는 RadioFeed로 들어와 잠깐 떴다 페이드.
// 발화자 있으면 초상화 표시(B), 없으면(시스템 경고) 초상화 숨김. 사망자=시체는 시체 에셋 나오면.
public class RadioWidget : MonoBehaviour
{
    [SerializeField] CanvasGroup _messageGroup;   // 메시지+초상화 페이드
    [SerializeField] TMP_Text    _message;
    [SerializeField] Image       _portrait;       // 발화자 초상화 (선택)
    [SerializeField] float       _showDuration = 3f;
    [SerializeField] float       _fadeSpeed    = 3f;

    float _timer;

    void OnEnable()  => RadioFeed.OnMessage += Show;
    void OnDisable() => RadioFeed.OnMessage -= Show;

    void Show(string msg, CharacterData speaker)
    {
        _message.text       = msg;
        _timer              = _showDuration;
        _messageGroup.alpha = 1f;

        if (_portrait != null)
        {
            bool has = speaker != null && speaker.portrait != null;
            _portrait.enabled = has;
            if (has) _portrait.sprite = speaker.portrait;
        }
    }

    void Update()
    {
        if (_timer > 0f) _timer -= Time.deltaTime;
        else _messageGroup.alpha = Mathf.MoveTowards(_messageGroup.alpha, 0f, _fadeSpeed * Time.deltaTime);
    }
}
