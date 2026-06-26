using UnityEngine;

// Player smooth follow. LateUpdate 채택 이유 — Player가 Update에서 움직인 후 따라감 (프레임 순서 떨림 방지).
// X/Y 분리 lerp 이유 — Y는 점프 따라가면 멀미. Y만 느리게 (멀미 완화 + 결국 따라감).
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] float _followSpeedX = 10f;
    [SerializeField] float _followSpeedY = 3f;

    void LateUpdate()
    {
        // null 가드 — Player Destroy 흐름 향후 대비 (현재 Revive는 GameObject 재활용이라 안전)
        if (_target == null) return;

        Vector3 pos = transform.position;
        Vector3 target = _target.position;
        // Time.deltaTime 곱 — fps 무관 일정 속도
        pos.x = Mathf.Lerp(pos.x, target.x, _followSpeedX * Time.deltaTime);
        pos.y = Mathf.Lerp(pos.y, target.y, _followSpeedY * Time.deltaTime);
        // z 보존 — Camera z=-10 유지 (target.z로 끌려가지 않게)
        transform.position = pos;
    }
}
