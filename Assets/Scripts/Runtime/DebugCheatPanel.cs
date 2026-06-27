using UnityEngine;
using UnityEngine.InputSystem;

// 런타임 디버그 치트 오버레이 — Play 중 캐릭터 교체 / 적 스폰 / 무적 / 무한탄.
// 에디터 스크립트 아님(런타임 MonoBehaviour, OnGUI + 핫키). 한달 데모 스코프 — 출시 전 제거.
public class DebugCheatPanel : MonoBehaviour
{
    [SerializeField] PlayerCharacterLoader _loader;       // 캐릭터 라이브 교체
    [SerializeField] CharacterRoster       _roster;       // 5종 마스터
    [SerializeField] PlayerHealth          _playerHealth; // 무적 토글
    [SerializeField] AbilityHandler        _ability;      // 탄약 치트
    [SerializeField] EnemySpawner          _spawner;      // 적 스폰
    [SerializeField] Camera                _camera;       // 마우스 월드 변환 (null이면 Camera.main)
    [SerializeField] bool                  _show = true;

    bool _infiniteAmmo;   // 패널 표시용 로컬 상태 (실제 적용은 _ability에 위임)

    bool Invincible => _playerHealth != null && _playerHealth.DebugInvincible;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;   // New Input System 미초기화 방어
        if (kb.f1Key.wasPressedThisFrame) _show = !_show;
        if (kb.gKey.wasPressedThisFrame)  ToggleInvincible();
        if (kb.nKey.wasPressedThisFrame)  ToggleInfiniteAmmo();
        if (kb.mKey.wasPressedThisFrame)  RefillAmmo();
        if (kb.bKey.wasPressedThisFrame)  SpawnAtMouse();
    }

    void OnGUI()
    {
        if (!_show) return;
        GUILayout.BeginArea(new Rect(10f, 10f, 230f, 360f), GUI.skin.box);
        GUILayout.Label("== DEBUG CHEAT (F1) ==");

        GUILayout.Label("─ 캐릭터 교체 ─");
        if (_loader != null && _roster != null && _roster.characters != null)
            foreach (var c in _roster.characters)
                if (c != null && GUILayout.Button($"▶ {c.characterName}"))
                    _loader.LoadCharacter(c);

        GUILayout.Space(6f);
        GUILayout.Label("─ 치트 ─");
        if (GUILayout.Button($"무적  [{(Invincible ? "ON" : "off")}]  (G)"))    ToggleInvincible();
        if (GUILayout.Button($"무한탄 [{(_infiniteAmmo ? "ON" : "off")}]  (N)")) ToggleInfiniteAmmo();
        if (GUILayout.Button("탄약 리필  (M)"))           RefillAmmo();
        if (GUILayout.Button("적 스폰 @마우스  (B)"))     SpawnAtMouse();

        GUILayout.EndArea();
    }

    void ToggleInvincible()
    {
        if (_playerHealth != null) _playerHealth.DebugInvincible = !_playerHealth.DebugInvincible;
    }

    void ToggleInfiniteAmmo()
    {
        _infiniteAmmo = !_infiniteAmmo;
        if (_ability != null) _ability.DebugSetInfiniteAmmo(_infiniteAmmo);
    }

    void RefillAmmo()
    {
        if (_ability != null) _ability.DebugRefillAmmo();
    }

    // 마우스 월드 위치에 적 1마리. 핫키 B = 커서 가리킨 곳, 버튼 = 패널 근처(커서가 버튼 위).
    void SpawnAtMouse()
    {
        if (_spawner == null || Mouse.current == null) return;
        Camera cam = _camera != null ? _camera : Camera.main;
        if (cam == null) return;
        Vector2 world = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _spawner.DebugSpawnAt(world);
    }
}
