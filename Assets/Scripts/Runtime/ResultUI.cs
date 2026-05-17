using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    // TMP_Text 사용 중 — 아트 에셋 준비 시 Image 기반 UI로 교체 가능
    [SerializeField] CanvasGroup       _panel;
    [SerializeField] TMP_Text          _scoreText;
    [SerializeField] TMP_Text          _coinText;
    [SerializeField] ContinueCountdown _countdown;
    [SerializeField] CanvasGroup       _fadeOverlay;
    [SerializeField] float             _gameOverFadeDuration = 10f;

    bool _acceptingInput;

    public event Action OnContinueRequested;
    public event Action OnGameOverComplete;

    void Awake()
    {
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;
        _countdown.OnComplete += OnCountdownComplete;
    }

    void OnDestroy()
    {
        _countdown.OnComplete -= OnCountdownComplete;
    }

    public void Show(float score, int coinCount)
    {
        _panel.alpha          = 1f;
        _panel.interactable   = true;
        _panel.blocksRaycasts = true;

        _scoreText.text = Mathf.FloorToInt(score).ToString();
        _coinText.text  = coinCount.ToString();

        if (coinCount > 0)
        {
            _acceptingInput = true;
            _countdown.StartCountdown();
        }
        else
        {
            StartCoroutine(GameOverSequence());
        }
    }

    public void Hide()
    {
        _acceptingInput = false;
        _countdown.StopCountdown();
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;
    }

    void Update()
    {
        if (_acceptingInput && Input.anyKeyDown)
        {
            _acceptingInput = false;
            OnContinueRequested?.Invoke();
        }
    }

    void OnCountdownComplete()
    {
        _acceptingInput = false;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        float elapsed = 0f;
        while (elapsed < _gameOverFadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadeOverlay.alpha = Mathf.Clamp01(elapsed / _gameOverFadeDuration);
            yield return null;
        }
        _fadeOverlay.alpha = 1f;
        OnGameOverComplete?.Invoke();
    }
}
