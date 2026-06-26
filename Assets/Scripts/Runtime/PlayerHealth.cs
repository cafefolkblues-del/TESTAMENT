using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int _maxHp = 1;

    public static event Action OnDeath;

    int  _currentHp;
    bool _isDead;

    // IDamageable — push 피해(적 발사체 등 향후)와 pull 피해가 같은 진영 판정 기준을 공유
    public Faction Faction => Faction.Player;

    void Start() => _currentHp = _maxHp;

    // pull 모델 — 접촉 피해(적 몸통)용. 상대 DamageSource를 읽어 아군이면 무시.
    void OnTriggerEnter2D(Collider2D other)
    {
        var src = other.GetComponent<DamageSource>();
        if (src == null) return;
        if (src.faction == Faction) return; // 동일 진영 피해 무시 (자해·아군 오사 방지)
        TakeDamage(src.damage);
    }

    // public + IDamageable 구현 — pull(자체 OnTrigger)과 push(외부 호출)가 같은 차감 경로를 타게 일원화
    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        int before = _currentHp;
        _currentHp -= amount;
        Debug.Log($"[Player] 피해 {amount} → HP {before}→{_currentHp}");   // 상태 디버그
        if (_currentHp <= 0) Die();
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        Debug.Log("[Player] 사망");   // 상태 디버그
        OnDeath?.Invoke();
    }

    public void Revive()
    {
        _isDead    = false;
        _currentHp = _maxHp;
        Debug.Log($"[Player] 부활 (HP {_maxHp})");   // 상태 디버그
    }
}
