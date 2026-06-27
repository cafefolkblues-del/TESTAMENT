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
    float          _aoeRadius;   // 0이면 단일 타격, >0이면 명중·최대사거리에서 범위 폭발(바주카)
    bool           _active;   // 반납 후 잔여 트리거/이중 반납 차단 가드

    // 풀에서 꺼낼 때 호출 — damage를 필드로 보관(DamageSource 미사용), 진행 방향으로 스프라이트 정렬.
    // speed·aoeRadius는 상류(무기)가 주입 — 일반탄=기존 속도·aoe0, 바주카=느린 속도·aoe>0.
    public void Launch(Vector2 origin, Vector2 dir, int damage, Faction faction, ProjectilePool pool, float speed, float aoeRadius)
    {
        transform.position = origin;
        _spawnPos  = origin;
        _dir       = dir.normalized;
        _damage    = damage;
        _faction   = faction;
        _pool      = pool;
        _speed     = speed;
        _aoeRadius = aoeRadius;
        _active    = true;

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
        {
            if (_aoeRadius > 0f) Explode(transform.position);   // 바주카: 빗맞아도 최대사거리서 공중폭발
            Despawn();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;
        var target = other.GetComponent<IDamageable>();
        if (target == null || target.Faction == _faction) return; // 비대상·아군 통과 (머즐 자해 포함)
        if (_aoeRadius > 0f) Explode(transform.position);          // 범위 폭발(명중점 기준)
        else                 target.TakeDamage(_damage);           // 단일 타격
        Despawn();   // first-hit 소멸
    }

    // 명중점 기준 범위 내 반대진영 전부 타격. 적중·최대사거리 공통 진입.
    void Explode(Vector2 center)
    {
        var hits = Physics2D.OverlapCircleAll(center, _aoeRadius);
        foreach (var h in hits)
        {
            var d = h.GetComponent<IDamageable>();
            if (d == null || d.Faction == _faction) continue;
            d.TakeDamage(_damage);
        }
    }

    void Despawn()
    {
        _active = false;
        _pool.Return(this);
    }
}
