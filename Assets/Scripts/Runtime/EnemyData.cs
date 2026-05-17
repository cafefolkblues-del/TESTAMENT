using UnityEngine;

public enum EnemyType
{
    Basic,
    Surveillance,
    Shield,
    Giant,
    Boarding,
    Sniper,
    Suicide,
}

public enum AttackPattern
{
    LinearCharge,
    AerialAssault,
    FixedAerialMissile,
    FrontBlockRush,
    QuadrupedBarrage,
    ClimbAndConvert,
    RearLaser,
    SuicideCharge,
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "TESTAMENT/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public EnemyType    enemyType;
    public int          hp;
    public float        moveSpeed;
    public float        spawnWeight;
    public AttackPattern attackPattern;
    public Sprite       sprite;
}
