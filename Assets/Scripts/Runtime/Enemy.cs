using UnityEngine;

// 데이터 구동 적 — EnemyData 하나로 자기 구성(hp/sprite/이동). attackPattern별 AI는 별도 단계.
//
// ⚠️ 임시: 이 적 프리팹에 붙는 DamageSource(Enemy)는 "접촉 딜" = 플레이어 사망 테스트용 placeholder다.
//    실제 적 공격은 attackPattern별 AI(직선 돌진/유도 미사일/탄막/스톰프 등)로 대체 예정 — 접촉 딜은 그때 제거.
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    EnemyHealth    _health;
    SpriteRenderer _sr;
    Rigidbody2D    _rb;
    float          _moveSpeed;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _sr     = GetComponent<SpriteRenderer>();
        _rb     = GetComponent<Rigidbody2D>();
    }

    // 스폰 직후 EnemySpawner가 호출 — EnemyData로 런타임 구성
    public void Init(EnemyData data)
    {
        _health.Init(data.hp);
        if (data.sprite != null) _sr.sprite = data.sprite;  // 없으면 프리팹 placeholder 스프라이트 유지
        _moveSpeed = data.moveSpeed;
    }

    // 좌향 행진 placeholder — 플레이어(좌측) 방향으로 단순 접근. attackPattern별 거동은 적 AI 단계에서 교체.
    // MovePosition: Kinematic RB라 물리 보간 유지하며 이동 (transform 직접 이동보다 트리거 감지 안정)
    void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + Vector2.left * _moveSpeed * Time.fixedDeltaTime);
    }
}
