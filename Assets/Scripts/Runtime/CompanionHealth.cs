using System;
using System.Collections;
using UnityEngine;

// 동료 전용 체력 — 플레이어(원샷 PlayerHealth) 재사용 안 함. HP 기반 + 피격 후 무적 점멸.
// 사망 대사/무전은 이 컴포넌트가 안 함 — OnAnyDeath만 쏘고, 대사 풀 단계에서 구독.
public class CompanionHealth : MonoBehaviour, IDamageable
{
    [SerializeField] SpriteRenderer _sr;
    [SerializeField] float _invulnDuration = 1f;    // 피격 후 무적 시간
    [SerializeField] float _blinkInterval  = 0.1f;  // 점멸 주기

    CompanionData _data;
    bool          _invuln;

    public Faction Faction => Faction.Player;

    // 대사/BGM 단계용 hook — 이번엔 발행만, 구독은 다음 단계
    public static event Action<CompanionData> OnAnyDeath;

    public void Init(CompanionData data) => _data = data;

    // 적 접촉 피해 pull (PlayerHealth와 동일 패턴) — 적 DamageSource만 수신, 아군 무시
    void OnTriggerEnter2D(Collider2D other)
    {
        var src = other.GetComponent<DamageSource>();
        if (src == null || src.faction == Faction) return;
        TakeDamage(src.damage);
    }

    // 가변 데미지 — 거대형/자폭형 강타 반영. 무적/사망 중엔 무시.
    public void TakeDamage(int amount)
    {
        if (_invuln || _data == null || !_data.isAlive) return;
        _data.currentHp -= amount;
        if (_data.currentHp <= 0) Die();
        else StartCoroutine(InvulnBlink());
    }

    // 피격 직후 i-frame — 다중 히트 방지 + 스프라이트 점멸
    IEnumerator InvulnBlink()
    {
        _invuln = true;
        float t = 0f;
        while (t < _invulnDuration)
        {
            if (_sr != null) _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(_blinkInterval);
            t += _blinkInterval;
        }
        if (_sr != null) _sr.enabled = true;
        _invuln = false;
    }

    void Die()
    {
        _data.isAlive = false;
        OnAnyDeath?.Invoke(_data);   // 사망 통보 (대사/무전은 다음 단계)
        Destroy(gameObject);
    }
}
