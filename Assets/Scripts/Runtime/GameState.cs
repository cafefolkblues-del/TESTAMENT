using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "TESTAMENT/Game State")]
public class GameState : ScriptableObject
{
    public float distanceScore;
    public int coinCount;
    public int waveCount;
    public List<CompanionData> companions;

    public void Reset()
    {
        distanceScore = 0f;
        coinCount     = 3;
        waveCount     = 0;
        foreach (var c in companions) c.Reset();
    }
}
