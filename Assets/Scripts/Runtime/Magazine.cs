using UnityEngine;

// 탄창 — 탄약만 게이트한다. 연사율(발사 간격)은 AbilityHandler 타이머 담당(여기 아님).
// 자동 장전: 빈 탄창 → reloadTime 후 보충. 전 탄창 항시 Tick(비활성 무기도 재장전 — 무기 저글 대응).
[System.Serializable]
public class Magazine
{
    public int   capacity   = 12;
    public float reloadTime = 1.5f;

    int   _ammo;
    bool  _reloading;
    float _timer;

    public bool DebugInfinite;   // 치트 — on이면 무한탄(게이트 통과·소비 무시)

    public bool CanFire   => DebugInfinite || (!_reloading && _ammo > 0);
    public int  Ammo      => _ammo;        // HUD용(추후)
    public bool Reloading => _reloading;

    public void Reset()
    {
        _ammo      = capacity;
        _reloading = false;
        _timer     = 0f;
    }

    // 발사 1회 소비. 가드: CanFire 게이트 없이 호출돼도 음수·이중장전 방지.
    public void Consume()
    {
        if (DebugInfinite) return;             // 치트 무한탄 — 소비 안 함
        if (_reloading || _ammo <= 0) return;
        if (--_ammo <= 0) { _reloading = true; _timer = reloadTime; }
    }

    public void Tick(float dt)
    {
        if (!_reloading) return;
        _timer -= dt;
        if (_timer <= 0f) { _ammo = capacity; _reloading = false; }
    }
}
