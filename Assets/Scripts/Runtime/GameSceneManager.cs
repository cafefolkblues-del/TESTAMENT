using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] GameState         _gameState;
    [SerializeField] CharacterRoster   _roster;
    [SerializeField] EnemySpawner      _spawner;
    [SerializeField] ResultUI          _resultUI;
    [SerializeField] PlayerController  _playerController;
    [SerializeField] PlayerHealth      _playerHealth;

    void Start()
    {
        PlayerHealth.OnDeath              += OnPlayerDeath;
        _resultUI.OnContinueRequested     += OnContinueRequested;
        _resultUI.OnGameOverComplete      += OnGameOverComplete;
        InitGame();
    }

    void OnDestroy()
    {
        PlayerHealth.OnDeath              -= OnPlayerDeath;
        _resultUI.OnContinueRequested     -= OnContinueRequested;
        _resultUI.OnGameOverComplete      -= OnGameOverComplete;
    }

    void InitGame()
    {
        _gameState.Reset();
        SpawnPlayer();
        InitCompanions();
    }

    void SpawnPlayer()
    {
        // PlayerController.Init(_selectedCharacter.character) — PlayerController 구현 시 연결
    }

    void InitCompanions()
    {
        int slot = 0;
        foreach (var character in _roster.characters)
        {
            if (character == _selectedCharacter.character) continue;
            if (slot >= _gameState.companions.Count) break;

            _gameState.companions[slot].character = character;
            _gameState.companions[slot].Reset();
            slot++;
        }
    }

    void OnPlayerDeath()
    {
        _playerController.SetInputEnabled(false);
        _spawner.Pause();
        _resultUI.Show(_gameState.distanceScore, _gameState.coinCount);
    }

    void OnContinueRequested()
    {
        _gameState.coinCount--;
        _playerHealth.Revive();
        _spawner.Resume();
        _playerController.SetInputEnabled(true);
        _resultUI.Hide();
    }

    void OnGameOverComplete()
    {
        _gameState.Reset();
        SceneManager.LoadScene("Start");
    }
}
