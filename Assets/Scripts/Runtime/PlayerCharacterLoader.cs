using UnityEngine;

public class PlayerCharacterLoader : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] SpriteRenderer _spriteRenderer;

    void Start()
    {
        if (_selectedCharacter.character == null) return;
        _spriteRenderer.sprite = _selectedCharacter.character.portrait;
    }

    public CharacterData Character => _selectedCharacter.character;
}
