using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] float inputDelay = 0.5f;
    [SerializeField] BlinkText _pressAnyKeyText;
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] float keyBlinkInterval = 0.1f;
    [SerializeField] float fadeDuration = 0.8f;

    bool _inputEnabled = false;
    IDisposable _anyPress;

    // onAnyButtonPress 채택 이유 — legacy Input.anyKeyDown과 의미가 1:1 동일
    // (디바이스 무관 "버튼 새로 눌림"). Update 폴링도 함께 제거.
    void Awake()
    {
        _anyPress = InputSystem.onAnyButtonPress.Call(OnAnyPress);
    }

    // 씬 전환 시 InputSystem 구독이 살아남지 않도록 해제
    void OnDestroy()
    {
        _anyPress?.Dispose();
    }

    // Invoke 채택 이유 — 0.5초 단발 지연 + 부가 로직 없음. 코루틴 1개 잡기보다 단순
    void Start() => Invoke(nameof(EnableInput), inputDelay);

    void EnableInput() => _inputEnabled = true;

    // legacy `if (_inputEnabled && Input.anyKeyDown)` 폴링을 이벤트 콜백으로 대체
    void OnAnyPress(InputControl _)
    {
        if (!_inputEnabled) return;
        _inputEnabled = false;
        StartCoroutine(PlayTransition());
    }

    IEnumerator PlayTransition()
    {
        // BlinkTimes는 IEnumerator를 반환 — yield return 한 줄로 chain (StartCoroutine 래핑 불필요)
        yield return _pressAnyKeyText.BlinkTimes(2, keyBlinkInterval);
        // 페이드는 CanvasGroupExtensions로 일원화 — 4중 복붙 제거의 일부
        yield return _fadeOverlay.Fade(0f, 1f, fadeDuration);
        // 마법 문자열 SceneManager.LoadScene 대신 SceneFlow — 흐름 단일 진입점
        SceneFlow.Load(SceneId.Tutorial);
    }
}
