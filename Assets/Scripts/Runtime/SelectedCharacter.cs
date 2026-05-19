using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCharacter", menuName = "TESTAMENT/Selected Character")]
public class SelectedCharacter : ScriptableObject
{
    public CharacterData character;

    // 매 런 새 선택을 강제 — 이전 런의 잔존 선택값 제거.
    // GameState.Reset 패턴과 동일 형태 (호출자 명시적 호출)
    public void Reset() => character = null;
}
