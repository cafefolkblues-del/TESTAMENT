using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;   // 수동조준 마우스 위치 읽기 (New Input System 채택)

public enum LucyWeapon { DualPistols, Spear, Gauntlet }

public class AbilityHandler : MonoBehaviour
{
    // fireRate 정정 — 기획서 정본: 헬 10발/초(0.1), 밀 2발/초(0.5). 기존 더미값 폐기.
    [SerializeField] float machineGunFireRate = 0.1f;
    [SerializeField] float revolverCooldown   = 0.5f;
    [SerializeField] int   shotgunPellets     = 6;
    [SerializeField] float shotgunSpread      = 30f;
    [SerializeField] float meleeRange         = 1.5f;
    [SerializeField] float meleeCooldown      = 0.4f;
    [SerializeField] float hackDuration       = 3f;
    [SerializeField] float chainsawDuration   = 5f;
    [SerializeField] float lucyMidThreshold   = 5f;
    [SerializeField] float lucyCloseThreshold = 2f;

    [Header("Projectile Wiring")]
    [SerializeField] ProjectilePool _projectilePool;
    [SerializeField] Transform      _firePoint;   // 머즐 위치 (null이면 transform 사용)
    [SerializeField] Camera         _camera;       // 수동조준 스크린→월드 변환용 (null이면 Camera.main)

    [Header("Damage (int, EnemyData.hp와 동일 단위)")]
    [SerializeField] int hellMachineGunDamage = 6;
    [SerializeField] int milRevolverDamage    = 18;
    [SerializeField] int milShotgunDamage     = 6;   // placeholder — 밀 샷건 dmg는 밸런스 패스에서 확정
    [SerializeField] float autoAimRange       = 20f;

    const Faction PlayerFaction = Faction.Player;

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

    // ── 미배선 stub (루시·릴 거리분기/근접용 — 별도 단계에서 구현) ──
    void FireProjectile(Transform target, float damage) { } // stub
    void FireSpread(int pellets, float spread)           { } // stub
    void MeleeHit(float range, float damage)             { } // stub

    // ── 발사 코어 (헬·밀 배선) ────────────────────────────────────

    Vector2 FirePoint => _firePoint ? (Vector2)_firePoint.position : (Vector2)transform.position;

    // 조준 방향 산출 — 발사체가 아닌 상류(여기)가 방향을 정해 주입한다.
    // auto: 최근접 적 방향, 타겟 없으면 false(발사 스킵). manual: 마우스 방향, 항상 true(퇴화 시 정면 fallback).
    bool TryAim(float range, bool manual, out Vector2 dir)
    {
        Vector2 origin = FirePoint;
        if (manual)
        {
            Camera cam = _camera ? _camera : Camera.main;
            Vector2 mouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            dir = mouse - origin;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right; // 마우스가 머즐과 겹치는 퇴화 케이스 방어
            dir.Normalize();
            return true;
        }

        var target = FindNearestEnemy(range);
        if (target == null) { dir = Vector2.zero; return false; }
        dir = ((Vector2)target.position - origin).normalized;
        return true;
    }

    // 단발 직선 — 풀에서 꺼내 방향·데미지 주입. 발사체는 DamageSource 없이 push로 피해를 줌.
    void FireStraight(Vector2 dir, int damage)
    {
        _projectilePool.Get().Launch(FirePoint, dir, damage, PlayerFaction, _projectilePool);
    }

    // 산탄 — baseDir 기준 콘 분산으로 N발. 펠릿마다 독립 발사체.
    void FireSpreadDir(Vector2 baseDir, int pellets, float spreadDeg, int damage)
    {
        float baseAng = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float start   = baseAng - spreadDeg * 0.5f;
        float step    = pellets > 1 ? spreadDeg / (pellets - 1) : 0f;
        for (int i = 0; i < pellets; i++)
        {
            float a = (start + step * i) * Mathf.Deg2Rad;
            FireStraight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), damage);
        }
    }

    // ── 헬 ────────────────────────────────────────────────────────

    void Fire_MachineGun()
    {
        if (_fireTimer > 0f) return;
        // 헬 우클릭 ManualAim 토글 시 마우스 방향, 아니면 자동조준
        if (!TryAim(autoAimRange, _manualAimActive, out var dir)) return;
        FireStraight(dir, hellMachineGunDamage);
        _fireTimer = machineGunFireRate;
    }

    void Toggle_ManualAim() => _manualAimActive = !_manualAimActive;

    // ── 밀 ────────────────────────────────────────────────────────

    void Fire_Revolver()
    {
        if (_cooldownTimer > 0f) return;
        if (!TryAim(autoAimRange, false, out var dir)) return; // 좌클릭 단발 = 자동조준
        FireStraight(dir, milRevolverDamage);
        _cooldownTimer = revolverCooldown;
    }

    // 밀 우클릭 = 마우스 수동조준 산탄. manual=true라 TryAim은 항상 방향을 반환.
    void Fire_Shotgun()
    {
        if (!TryAim(autoAimRange, true, out var dir)) return;
        FireSpreadDir(dir, shotgunPellets, shotgunSpread, milShotgunDamage);
    }

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
