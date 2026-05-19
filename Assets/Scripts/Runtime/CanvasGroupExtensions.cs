using System.Collections;
using UnityEngine;

// 페이드 패턴이 Start/Tutorial/CharacterSelect/Result 4중 복붙되어 있던 것 통합.
// 확장 메서드로 노출 — 호출 측은 `yield return _overlay.Fade(0, 1, 0.8f)` 한 줄로 chain.
public static class CanvasGroupExtensions
{
    // duration 동안 alpha를 from→to로 보간. 호출 코루틴에서 yield return으로 대기.
    public static IEnumerator Fade(this CanvasGroup group, float from, float to, float duration)
    {
        group.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Lerp + Clamp01 조합 이유 — t가 duration 초과해도 alpha가 정확히 to에서 멈추도록 정규화
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = to;
    }
}
