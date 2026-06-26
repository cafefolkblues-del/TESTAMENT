using UnityEngine;

public enum DialogueContext { Combat, Lull, Stress }

// 직전 발화자별 추가 배율 — 릴 리액션 윈도우 hook (데이터 구동, 릴 프로필만 채움. 비면 1배)
[System.Serializable]
public struct ReactionBoost
{
    public CharacterData speaker;
    public float         mult;
}

// 캐릭터별 대사 데이터 — 발화 빈도/상황 배율/컨텍스트별 대사 풀. 코드와 콘텐츠 분리.
[CreateAssetMenu(fileName = "DialogueProfile", menuName = "TESTAMENT/Dialogue Profile")]
public class DialogueProfile : ScriptableObject
{
    [Header("발화 빈도 (성격)")]
    public float baseFrequency = 1f;

    [Header("상황 배율 (placeholder — 인스펙터 튜닝)")]
    public float combatMult = 1f;
    public float crisisMult = 1f;   // 위기 = Stress
    public float lullMult   = 1f;

    [Header("대사 풀")]
    [TextArea] public string[] combatLines;
    [TextArea] public string[] crisisLines;
    [TextArea] public string[] lullLines;

    [Header("거대형 경고 (트리거 — 생존 동료가 예고)")]
    [TextArea] public string[] giantWarningLines;

    [Header("동료 사망 반응 (트리거 fallback — 특정 반응 없을 때)")]
    [TextArea] public string[] deathReactionLines;

    [Header("리액션 윈도우 hook (릴만 채움)")]
    public ReactionBoost[] reactionBoosts;

    public string[] LinesFor(DialogueContext c)
    {
        if (c == DialogueContext.Combat) return combatLines;
        if (c == DialogueContext.Stress) return crisisLines;
        return lullLines;
    }

    public float MultFor(DialogueContext c)
    {
        if (c == DialogueContext.Combat) return combatMult;
        if (c == DialogueContext.Stress) return crisisMult;
        return lullMult;
    }
}
