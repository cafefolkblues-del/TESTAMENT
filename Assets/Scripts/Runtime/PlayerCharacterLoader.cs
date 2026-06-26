using UnityEngine;

public class PlayerCharacterLoader : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] SpriteRenderer _spriteRenderer;
    // Init 호출 위해 추가 — _moveSpeed/_jumpForce/AbilityHandler 셋업 진입점
    [SerializeField] PlayerController _playerController;

    // Start가 아닌 public 메서드 — 초기화 시점을 GameSceneManager가 통제(Reset→Load→Companions 순서 보장).
    // Loader는 "어떻게 로드하나"만 담당, "언제 로드하나"는 Manager 책임.
    public void Load()
    {
        if (_selectedCharacter.character == null) return;
        _spriteRenderer.sprite = _selectedCharacter.character.portrait;
        // Init 호출 안 하면 _moveSpeed=0이라 PlayerController.Update의 early return 발동 → 이동 안 됨
        _playerController.Init(_selectedCharacter.character);
    }

    public CharacterData Character => _selectedCharacter.character;
}
