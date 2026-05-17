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
    ManualAim,
    ShotgunModeHold,
    ManualWeaponSwap,
    GuardAndCharge,
    EnemyHack,
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
}
