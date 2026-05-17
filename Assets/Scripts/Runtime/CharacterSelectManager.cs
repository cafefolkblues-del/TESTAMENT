using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] Transform _panelContainer;
    [SerializeField] CanvasGroup _fadeOverlay;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] string nextSceneName;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] float fadeDuration = 0.8f;
    [SerializeField] float titleCharInterval = 0.05f;

    CharacterPanel[] _panels;

    void Start()
    {
        _panels = _panelContainer.GetComponentsInChildren<CharacterPanel>();
        foreach (var p in _panels) p.Init(this);
        StartCoroutine(AnimateTitle());
    }

    IEnumerator AnimateTitle()
    {
        _titleText.maxVisibleCharacters = 0;
        _titleText.ForceMeshUpdate();
        int total = _titleText.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            _titleText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(titleCharInterval);
        }
    }

    public void OnPanelSelected(CharacterPanel panel)
    {
        _selectedCharacter.character = panel.Data;
        foreach (var p in _panels) p.Lock();
        StartCoroutine(PlayTransition(panel));
    }

    IEnumerator PlayTransition(CharacterPanel panel)
    {
        yield return StartCoroutine(panel.BlinkWhite(2, blinkInterval));

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
