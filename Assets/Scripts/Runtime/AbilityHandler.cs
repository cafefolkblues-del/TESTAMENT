using System.Collections;
using UnityEngine;

public enum LucyWeapon { DualPistols, Spear, Gauntlet }

public class AbilityHandler : MonoBehaviour
{
    [SerializeField] float machineGunFireRate = 0.08f;
    [SerializeField] float revolverCooldown   = 0.35f;
    [SerializeField] int   shotgunPellets     = 6;
    [SerializeField] float shotgunSpread      = 30f;
    [SerializeField] float meleeRange         = 1.5f;
    [SerializeField] float meleeCooldown      = 0.4f;
    [SerializeField] float hackDuration       = 3f;
    [SerializeField] float chainsawDuration   = 5f;
    [SerializeField] float lucyMidThreshold   = 5f;
    [SerializeField] float lucyCloseThreshold = 2f;

    AbilityType _left;
    AbilityType _right;
    float      _fireTimer;
    float      _cooldownTimer;
    bool       _manualAimActive;
    bool       _chainsawEnhanced;
    float      _chainsawTimer;
    LucyWeapon _lucyWeapon;

    public void Init(AbilityType left, AbilityType right)
    {
        _left             = left;
        _right            = right;
        _fireTimer        = 0f;
        _cooldownTimer    = 0f;
        _chainsawEnhanced = false;
        _lucyWeapon       = LucyWeapon.DualPistols;
    }

    void Update()
    {
        _fireTimer     -= Time.deltaTime;
        _cooldownTimer -= Time.deltaTime;
        UpdateChainsawTimer();
        if (_left == AbilityType.AutoWeaponSwap) UpdateLucyWeapon();
    }

    // ── 진입점 ────────────────────────────────────────────────────

    public void OnLeftDown()
    {
        switch (_left)
        {
            case AbilityType.RevolverSingle:  Fire_Revolver();     break;
            case AbilityType.AutoWeaponSwap:  Fire_LucyCurrent();  break;
            case AbilityType.MeleeAutoHit:    MeleeAutoHit_Ril();  break;
            case AbilityType.HackingTool:     /* TBD */            break;
        }
    }

    public void OnLeftHeld()
    {
        switch (_left)
        {
            case AbilityType.MachineGunExplosive: Fire_MachineGun(); break;
        }
    }

    public void OnLeftUp() { }

    public void OnRightDown()
    {
        switch (_right)
        {
            case AbilityType.ManualAim:        Toggle_ManualAim();                       break;
            case AbilityType.ShotgunModeHold:  Fire_Shotgun();                           break;
            case AbilityType.ManualWeaponSwap: Swap_ManualLucy();                        break;
            case AbilityType.GuardAndCharge:   StartCoroutine(DashParry_Ril());          break;
            case AbilityType.EnemyHack:        HackEnemy_Vil();                          break;
        }
    }

    public void OnRightHeld() { }
    public void OnRightUp()   { }

    // ── 공통 유틸 ─────────────────────────────────────────────────

    Transform FindNearestEnemy(float range)
    {
        var hits    = Physics2D.OverlapCircleAll(transform.position, range);
        Transform nearest = null;
        float minDist     = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; nearest = h.transform; }
        }
        return nearest;
    }

    void FireProjectile(Transform target, float damage) { } // stub
    void FireSpread(int pellets, float spread)           { } // stub
    void MeleeHit(float range, float damage)             { } // stub

    // ── 헬 ────────────────────────────────────────────────────────

    void Fire_MachineGun()
    {
        if (_fireTimer > 0f) return;
        var target = FindNearestEnemy(20f);
        if (target == null) return;
        FireProjectile(target, 1f);
        _fireTimer = machineGunFireRate;
    }

    void Toggle_ManualAim() => _manualAimActive = !_manualAimActive;

    // ── 밀 ────────────────────────────────────────────────────────

    void Fire_Revolver()
    {
        if (_cooldownTimer > 0f) return;
        var target = FindNearestEnemy(20f);
        if (target == null) return;
        FireProjectile(target, 1f);
        _cooldownTimer = revolverCooldown;
    }

    void Fire_Shotgun() => FireSpread(shotgunPellets, shotgunSpread);

    // ── 루시 ──────────────────────────────────────────────────────

    void UpdateLucyWeapon()
    {
        var nearest = FindNearestEnemy(100f);
        if (nearest == null) return;
        float dist = Vector2.Distance(transform.position, nearest.position);
        if      (dist > lucyMidThreshold)   _lucyWeapon = LucyWeapon.DualPistols;
        else if (dist > lucyCloseThreshold) _lucyWeapon = LucyWeapon.Spear;
        else                                _lucyWeapon = LucyWeapon.Gauntlet;
    }

    void Fire_LucyCurrent()
    {
        switch (_lucyWeapon)
        {
            case LucyWeapon.DualPistols: FireProjectile(FindNearestEnemy(20f), 0.5f); break;
            case LucyWeapon.Spear:       FireProjectile(FindNearestEnemy(10f), 1.0f); break;
            case LucyWeapon.Gauntlet:    MeleeHit(meleeRange, 2.0f);                  break;
        }
    }

    void Swap_ManualLucy() =>
        _lucyWeapon = (LucyWeapon)(((int)_lucyWeapon + 1) % 3);

    // ── 릴 ────────────────────────────────────────────────────────

    void MeleeAutoHit_Ril()
    {
        if (_cooldownTimer > 0f) return;
        MeleeHit(meleeRange, _chainsawEnhanced ? 3f : 1f);
        _cooldownTimer = meleeCooldown;
    }

    IEnumerator DashParry_Ril()
    {
        float dashDuration = 0.3f;
        float elapsed      = 0f;
        // stub: 무적 ON + 돌진 velocity 적용 필요
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var h in hits)
            {
                if (!h.CompareTag("EnemyProjectile")) continue;
                _chainsawEnhanced = true;
                _chainsawTimer    = chainsawDuration;
                Destroy(h.gameObject);
                yield break;
            }
            yield return null;
        }
        // stub: 무적 OFF
    }

    void UpdateChainsawTimer()
    {
        if (!_chainsawEnhanced) return;
        _chainsawTimer -= Time.deltaTime;
        if (_chainsawTimer <= 0f) _chainsawEnhanced = false;
    }

    // ── 빌 ────────────────────────────────────────────────────────

    // 좌클릭: TBD

    void HackEnemy_Vil()
    {
        var target = FindNearestEnemy(15f);
        if (target == null) return;
        StartCoroutine(HackTimer(target));
    }

    IEnumerator HackTimer(Transform enemy)
    {
        if (enemy == null) yield break;
        enemy.tag = "Ally";
        yield return new WaitForSeconds(hackDuration);
        if (enemy != null) enemy.tag = "Enemy";
    }
}
