using UnityEngine;

[CreateAssetMenu(fileName = "CompanionData", menuName = "TESTAMENT/Companion Data")]
public class CompanionData : ScriptableObject
{
    public CharacterData character;
    public int hitCount;
    public bool isAlive;

    public void Reset()
    {
        hitCount = 0;
        isAlive = true;
    }
}
