using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSceneManager : MonoBehaviour
{
    [SerializeField] string nextSceneName = "CharacterSelect";
    [SerializeField] CanvasGroup _tutorialImage;
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] float fadeInDuration  = 0.6f;
    [SerializeField] float displayDuration = 5f;
    [SerializeField] float fadeOutDuration = 0.8f;

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        yield return StartCoroutine(Fade(_tutorialImage, 0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(Fade(_fadeOverlay, 0f, 1f, fadeOutDuration));
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = to;
    }
}
