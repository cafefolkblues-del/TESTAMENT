using UnityEngine;

[CreateAssetMenu(fileName = "CompanionData", menuName = "TESTAMENT/Companion Data")]
public class CompanionData : ScriptableObject
{
    public CharacterData character;
    public int  maxHp = 3;   // 동료 체력 — 플레이 중 조정 (GDD)
    public int  currentHp;
    public bool isAlive;

    // 체력 상태를 SO에 보관 — 엔티티 파괴 후에도 무전/UI가 생사·HP 읽음
    public void Reset()
    {
        currentHp = maxHp;
        isAlive   = true;
    }
}
