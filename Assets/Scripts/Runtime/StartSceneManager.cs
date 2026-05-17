using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] string nextSceneName = "CharacterSelect";
    [SerializeField] float inputDelay = 0.5f;
    [SerializeField] BlinkText _pressAnyKeyText;
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] float keyBlinkInterval = 0.1f;
    [SerializeField] float fadeDuration = 0.8f;

    bool _inputEnabled = false;

    void Start()
    {
        Invoke(nameof(EnableInput), inputDelay);
    }

    void EnableInput() => _inputEnabled = true;

    void Update()
    {
        if (!_inputEnabled) return;
        if (Input.anyKeyDown)
        {
            _inputEnabled = false;
            StartCoroutine(PlayTransition());
        }
    }

    IEnumerator PlayTransition()
    {
        yield return StartCoroutine(_pressAnyKeyText.BlinkTimes(2, keyBlinkInterval));

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadeOverlay.alpha = elapsed / fadeDuration;
            yield return null;
        }
        _fadeOverlay.alpha = 1f;

        SceneManager.LoadScene(nextSceneName);
    }
}
