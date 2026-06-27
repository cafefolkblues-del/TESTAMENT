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
    [SerializeField] float shotgunCooldown    = 0.6f;   // 밀 우클릭 홀드 산탄 간격 (홀드 폭주 방지)
    [SerializeField] float meleeRange         = 1.5f;
    [SerializeField] float meleeCooldown      = 0.4f;
    [SerializeField] float _multiHitInterval  = 0.1f;   // 근접 다단 히트 간격
    [SerializeField] float hackDuration       = 3f;
    [SerializeField] float chainsawDuration   = 5f;
    [SerializeField] float lucyMidThreshold   = 5f;
    [SerializeField] float lucyCloseThreshold = 2f;

    [Header("Projectile Wiring")]
    [SerializeField] ProjectilePool _projectilePool;
    [SerializeField] Transform      _firePoint;   // 머즐 위치 (null이면 transform 사용)
    [SerializeField] Camera         _camera;       // 수동조준 스크린→월드 변환용 (null이면 Camera.main)
    [SerializeField] float          bulletSpeed = 18f;   // 일반탄 속도 (바주카 등 특수탄은 각 캐릭터 턴에서 별도 주입)

    // 탄창 — 검증 스캐폴드(명명 필드). 3번째 캐릭터 전에 무기별 SO로 이전 예정.
    [Header("Magazine")]
    [SerializeField] Magazine hellMgMag;
    [SerializeField] Magazine bazookaMag;   // 헬 우클 — capacity ~4

    [Header("헬 바주카 (우클)")]
    [SerializeField] float bazookaCooldown = 1.0f;   // 발사 간격 (느린 신중 발사)
    [SerializeField] int   bazookaDamage   = 30;     // placeholder — 밸런스 패스 확정
    [SerializeField] float bazookaSpeed    = 8f;     // 일반탄(18)보다 느림
    [SerializeField] float bazookaAoe      = 1.5f;   // 폭발 반경

    [Header("Damage (int, EnemyData.hp와 동일 단위)")]
    [SerializeField] int hellMachineGunDamage = 6;
    [SerializeField] int milRevolverDamage    = 18;
    [SerializeField] int milShotgunDamage     = 6;   // placeholder — 밀 샷건 dmg는 밸런스 패스에서 확정
    [SerializeField] float autoAimRange       = 20f;

    const Faction PlayerFaction = Faction.Player;

    AbilityType _left;
    AbilityType _right;
    float      _fireTimer;       // 헬 기관총 연사 간격 (좌·우 공용 — 같은 총)
    float      _cooldownTimer;   // 밀 리볼버 단발 쿨 (좌)
    float      _shotgunTimer;    // 밀 샷건 홀드 간격 (우)
    float      _bazookaTimer;    // 헬 바주카 발사 간격 (우)
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
        hellMgMag.Reset();
        bazookaMag.Reset();
    }

    // ── 치트 훅 (DebugCheatPanel용) ──
    // TODO: 캐릭터 탄창 추가 시(밀 리볼버/샷건, 빌 해킹툴 등) 여기 모두 등록.
    void ForEachMag(System.Action<Magazine> op) { op(hellMgMag); op(bazookaMag); }
    public void DebugRefillAmmo()              => ForEachMag(m => m.Reset());
    public void DebugSetInfiniteAmmo(bool on)  => ForEachMag(m => m.DebugInfinite = on);

    void Update()
    {
        _fireTimer     -= Time.deltaTime;
        _cooldownTimer -= Time.deltaTime;
        _shotgunTimer  -= Time.deltaTime;
        _bazookaTimer  -= Time.deltaTime;
        hellMgMag.Tick(Time.deltaTime);   // 전 탄창 항시 Tick (비활성 무기도 재장전)
        bazookaMag.Tick(Time.deltaTime);
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

    // 좌클릭 = 자동조준 발사 (홀드 연사 무기)
    public void OnLeftHeld()
    {
        switch (_left)
        {
            case AbilityType.MachineGunExplosive: Fire_MachineGun(manual: false); break;
        }
    }

    public void OnLeftUp() { }

    // 우클릭 = 수동조준 발사. 연사/홀드 무기는 OnRightHeld에서 처리(아래).
    // 토글 아님 — 누르는 동안 마우스 방향으로 직접 발사. 루시/릴/빌 special만 Down에 잔류.
    public void OnRightDown()
    {
        switch (_right)
        {
            case AbilityType.ManualWeaponSwap: Swap_ManualLucy();                break;
            case AbilityType.GuardAndCharge:   StartCoroutine(DashParry_Ril());  break;
            case AbilityType.EnemyHack:        HackEnemy_Vil();                  break;
            case AbilityType.Bazooka:          Fire_Bazooka();                   break; // 헬 우클 단발

        }
    }

    // 우클릭 홀드 = 수동조준 연사 (헬 기관총 / 밀 샷건)
    public void OnRightHeld()
    {
        switch (_right)
        {
            case AbilityType.ManualAim:       Fire_MachineGun(manual: true); break; // 헬 수동 기관총
            case AbilityType.ShotgunModeHold: Fire_Shotgun(manual: true);   break; // 밀 수동 샷건
        }
    }

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

    // ── 미배선 stub (루시 거리분기 발사용 — 루시 턴에서 구현) ──
    void FireProjectile(Transform target, float damage) { } // stub
    void FireSpread(int pellets, float spread)           { } // stub

    // ── 근접 타격 capability (단일=hits1 / 다단=hits N) — 루시 건틀릿·릴 좌클 사용 ──
    void MeleeHit(float range, int damage, int hits = 1) => StartCoroutine(MeleeRoutine(range, damage, hits));

    IEnumerator MeleeRoutine(float range, int damage, int hits)
    {
        for (int i = 0; i < hits; i++)
        {
            var targets = Physics2D.OverlapCircleAll(transform.position, range);
            foreach (var t in targets)
            {
                var d = t.GetComponent<IDamageable>();
                if (d == null || d.Faction == PlayerFaction) continue;   // 비대상·아군 통과
                d.TakeDamage(damage);
            }
            if (i < hits - 1) yield return new WaitForSeconds(_multiHitInterval);
        }
    }

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

    // 단발 직선 — 풀에서 꺼내 방향·데미지·속도·범위 주입. 발사체는 DamageSource 없이 push로 피해를 줌.
    void FireStraight(Vector2 dir, int damage, float speed, float aoe)
    {
        _projectilePool.Get().Launch(FirePoint, dir, damage, PlayerFaction, _projectilePool, speed, aoe);
    }

    // 산탄 — baseDir 기준 콘 분산으로 N발. 펠릿마다 독립 발사체.
    void FireSpreadDir(Vector2 baseDir, int pellets, float spreadDeg, int damage, float speed, float aoe)
    {
        float baseAng = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float start   = baseAng - spreadDeg * 0.5f;
        float step    = pellets > 1 ? spreadDeg / (pellets - 1) : 0f;
        for (int i = 0; i < pellets; i++)
        {
            float a = (start + step * i) * Mathf.Deg2Rad;
            FireStraight(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), damage, speed, aoe);
        }
    }

    // ── 헬 ────────────────────────────────────────────────────────

    // 헬 기관총 — manual: 좌클릭=false(자동조준), 우클릭=true(마우스)
    void Fire_MachineGun(bool manual)
    {
        if (_fireTimer > 0f) return;            // 연사율 게이트
        if (!hellMgMag.CanFire) return;         // 탄약 게이트 (빈 탄창/장전중 발사 X)
        if (!TryAim(autoAimRange, manual, out var dir)) return; // 자동 시 타겟 없으면 스킵
        FireStraight(dir, hellMachineGunDamage, bulletSpeed, 0f);
        hellMgMag.Consume();
        _fireTimer = machineGunFireRate;
    }

    // 헬 바주카 — 우클 단발 수동조준. 느린 speed + aoe로 범위 폭발(코어 Projectile capability 사용).
    void Fire_Bazooka()
    {
        if (_bazookaTimer > 0f) return;          // 발사 간격 게이트
        if (!bazookaMag.CanFire) return;         // 탄약 게이트
        if (!TryAim(autoAimRange, manual: true, out var dir)) return;  // 수동조준(마우스)
        FireStraight(dir, bazookaDamage, bazookaSpeed, bazookaAoe);
        bazookaMag.Consume();
        _bazookaTimer = bazookaCooldown;
    }

    // ── 밀 ────────────────────────────────────────────────────────

    void Fire_Revolver()
    {
        if (_cooldownTimer > 0f) return;
        if (!TryAim(autoAimRange, false, out var dir)) return; // 좌클릭 단발 = 자동조준
        FireStraight(dir, milRevolverDamage, bulletSpeed, 0f);
        _cooldownTimer = revolverCooldown;
    }

    // 밀 샷건 — 우클릭 홀드 수동조준 산탄. shotgunCooldown으로 홀드 연사 간격 제한.
    void Fire_Shotgun(bool manual)
    {
        if (_shotgunTimer > 0f) return;
        if (!TryAim(autoAimRange, manual, out var dir)) return;
        FireSpreadDir(dir, shotgunPellets, shotgunSpread, milShotgunDamage, bulletSpeed, 0f);
        _shotgunTimer = shotgunCooldown;
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
            case LucyWeapon.Gauntlet:    MeleeHit(meleeRange, 2);                     break;   // 임시 dmg — 루시 턴 확정
        }
    }

    void Swap_ManualLucy() =>
        _lucyWeapon = (LucyWeapon)(((int)_lucyWeapon + 1) % 3);

    // ── 릴 ────────────────────────────────────────────────────────

    void MeleeAutoHit_Ril()
    {
        if (_cooldownTimer > 0f) return;
        MeleeHit(meleeRange, _chainsawEnhanced ? 3 : 1);   // 임시 dmg — 릴 턴 확정
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
