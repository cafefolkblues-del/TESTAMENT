using UnityEngine;

// 동료 엔티티 — 이번 단계는 플레이어 추종 이동만. 전투·조준은 별도(아군 AI 조준 시스템) 후속.
public class Companion : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sr;
    [SerializeField] float _followSpeed = 6f;

    Transform _player;
    Vector2   _offset;   // 플레이어 기준 포메이션 위치

    public void Init(CompanionData data, Transform player, Vector2 formationOffset)
    {
        _player = player;
        _offset = formationOffset;
        if (data.character != null && data.character.portrait != null) _sr.sprite = data.character.portrait;
    }

    void Update()
    {
        if (_player == null) return;   // 플레이어 사망/파괴 대비 가드
        // 포메이션 위치로 부드럽게 추종 (fps 무관 Lerp)
        Vector3 target = _player.position + (Vector3)_offset;
        transform.position = Vector3.Lerp(transform.position, target, _followSpeed * Time.deltaTime);
    }
}
