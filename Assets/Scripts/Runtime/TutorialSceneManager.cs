using System.Collections;
using UnityEngine;

public class TutorialSceneManager : MonoBehaviour
{
    [SerializeField] CanvasGroup _tutorialImage;
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] float fadeInDuration  = 0.6f;
    [SerializeField] float displayDuration = 5f;
    [SerializeField] float fadeOutDuration = 0.8f;

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        // 인스펙터 누락 시에도 일관 동작하도록 시작 alpha 명시 박음 (방어적 초기화)
        _tutorialImage.alpha = 0f;
        _fadeOverlay.alpha   = 0f;

        // CanvasGroupExtensions.Fade로 일원화 — 자체 Fade 코루틴 삭제
        yield return _tutorialImage.Fade(0f, 1f, fadeInDuration);
        // WaitForSeconds 채택 이유 — 단순 고정 대기. Time.deltaTime 누적 루프 불필요
        yield return new WaitForSeconds(displayDuration);
        yield return _fadeOverlay.Fade(0f, 1f, fadeOutDuration);

        SceneFlow.Load(SceneId.CharacterSelect);
    }
}
