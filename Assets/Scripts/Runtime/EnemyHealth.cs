using UnityEngine;

// PlayerHealth 미러 — 적 피해 수신부. 이번 단계엔 발사체(push)로만 피해를 받으므로 OnTrigger 없음.
// (접촉 피해 pull은 플레이어 쪽 책임. 적이 적을 때리는 일은 없음 — faction 판정으로 차단됨)
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int _maxHp = 20;

    int  _currentHp;
    bool _isDead;

    public Faction Faction => Faction.Enemy;

    // 인스펙터 placeholder값 사용 (스폰 단계에서 Init으로 EnemyData.hp 주입 예정)
    void Awake() => _currentHp = _maxHp;

    // EnemyData.hp 주입 진입점 — 풀에서 재사용 시 상태 리셋도 겸함
    public void Init(int maxHp)
    {
        _maxHp     = maxHp;
        _currentHp = maxHp;
        _isDead    = false;
    }

    // 발사체 push 호출 — 진영 판정은 호출자(Projectile)가 이미 끝냄. 여기선 차감만.
    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        _currentHp -= amount;
        if (_currentHp <= 0) Die();
    }

    // 적은 파괴 (오브젝트 풀링은 적 스폰 단계에서 도입)
    void Die()
    {
        _isDead = true;
        Destroy(gameObject);
    }
}
