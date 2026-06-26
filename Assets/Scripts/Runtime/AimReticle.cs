using UnityEngine;
using UnityEngine.InputSystem;

// 수동조준 reticle — 우클릭(우마우스 held) 동안 마우스 위치에 십자선 표시.
// AbilityHandler 의존 없이 우마우스 상태만 읽음(수동조준=우클릭 규칙과 일치). Overlay 캔버스라 position=스크린 px.
public class AimReticle : MonoBehaviour
{
    [SerializeField] RectTransform _reticle;   // 십자선 루트 (평소 비활성)

    void Update()
    {
        var m = Mouse.current;
        bool aiming = m != null && m.rightButton.isPressed;

        if (_reticle.gameObject.activeSelf != aiming) _reticle.gameObject.SetActive(aiming);
        if (aiming) _reticle.position = m.position.ReadValue();
    }
}
