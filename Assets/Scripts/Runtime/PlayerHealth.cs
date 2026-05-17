using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int _maxHp = 1;

    public static event Action OnDeath;

    int  _currentHp;
    bool _isDead;

    void Start() => _currentHp = _maxHp;

    void OnTriggerEnter2D(Collider2D other)
    {
        var src = other.GetComponent<DamageSource>();
        if (src == null) return;
        TakeDamage(src.damage);
    }

    void TakeDamage(int amount)
    {
        if (_isDead) return;
        _currentHp -= amount;
        if (_currentHp <= 0) Die();
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        OnDeath?.Invoke();
    }

    public void Revive()
    {
        _isDead    = false;
        _currentHp = _maxHp;
    }
}
