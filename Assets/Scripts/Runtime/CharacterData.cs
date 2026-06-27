using UnityEngine;

public enum AbilityType
{
    // 좌클릭
    MachineGunExplosive,
    RevolverSingle,
    AutoWeaponSwap,
    MeleeAutoHit,
    HackingTool,
    // 우클릭
    ManualAim,        // (구 헬 수동기관총 — 미사용, enum 값 유지 위해 잔존)
    ShotgunModeHold,
    ManualWeaponSwap,
    GuardAndCharge,
    EnemyHack,
    Bazooka,          // 헬 우클 — 느린 폭발 로켓(끝에 append, 기존 값 보존)
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "TESTAMENT/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public float moveSpeed;
    public float jumpForce;
    public AbilityType leftAbility;
    public AbilityType rightAbility;
    public DialogueProfile dialogue;   // 대사 데이터 링크 (발화 빈도/배율/대사 풀)
}
