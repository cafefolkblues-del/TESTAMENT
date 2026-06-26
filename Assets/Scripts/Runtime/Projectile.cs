using UnityEngine;

// 직선 발사체 (push 모델). 방향은 상류(조준)에서 주입받고, 발사체는 그 방향으로 직진만 한다 — 스스로 타겟을 고르지 않음.
// 피해+소멸을 자기 OnTrigger 한 콜백에서 원자적으로 처리 → 수신부와의 이중 OnTrigger 경합 제거.
public class Projectile : MonoBehaviour
{
    [SerializeField] float _speed       = 18f;
    [SerializeField] float _maxDistance = 25f;   // 프리팹 공유값 (무기별 분화는 아트/프리팹 분리 단계에서)

    Vector2        _dir;
    Vector2        _spawnPos;
    int            _damage;
    Faction        _faction;
    ProjectilePool _pool;
    bool           _active;   // 반납 후 잔여 트리거/이중 반납 차단 가드

    // 풀에서 꺼낼 때 호출 — damage를 필드로 보관(DamageSource 미사용), 진행 방향으로 스프라이트 정렬
    public void Launch(Vector2 origin, Vector2 dir, int damage, Faction faction, ProjectilePool pool)
    {
        transform.position = origin;
        _spawnPos = origin;
        _dir      = dir.normalized;
        _damage   = damage;
        _faction  = faction;
        _pool     = pool;
        _active   = true;

        float ang = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!_active) return;
        transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
        // sqrMagnitude 비교 — 매 프레임 sqrt 회피
        if (((Vector2)transform.position - _spawnPos).sqrMagnitude >= _maxDistance * _maxDistance)
            Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;
        var target = other.GetComponent<IDamageable>();
        if (target == null || target.Faction == _faction) return; // 비대상·아군 통과 (머즐 자해 포함)
        target.TakeDamage(_damage);
        Despawn();   // first-hit 소멸
    }

    void Despawn()
    {
        _active = false;
        _pool.Return(this);
    }
}
