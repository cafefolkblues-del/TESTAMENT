using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] SelectedCharacter _selectedCharacter;
    [SerializeField] GameState         _gameState;
    [SerializeField] CharacterRoster   _roster;
    [SerializeField] EnemySpawner      _spawner;
    [SerializeField] DistanceScoreTracker _scoreTracker;   // 거리 스코어 — 사망 중 정지 제어
    [SerializeField] ResultUI          _resultUI;
    [SerializeField] ContinueCountdown _countdown;       // ResultUI에서 이관 — 사망 흐름 통제 통합
    [SerializeField] PlayerCharacterLoader _playerLoader; // 플레이어 로드 진입점 — Loader.Start 대신 Manager가 호출
    [SerializeField] PlayerController  _playerController;
    [SerializeField] PlayerHealth      _playerHealth;
    [SerializeField] CanvasGroup       _fadeOverlay;     // 게임오버 페이드 — Game 씬의 글로벌 오버레이
    [SerializeField] float             _gameOverFadeDuration = 10f;

    IDisposable _anyPress;

    void Start()
    {
        // 사망 이벤트는 PlayerHealth static event로 들어오고, 카운트다운 만료는 ContinueCountdown 이벤트.
        // 두 이벤트만 받아 흐름 분기 — UI(ResultUI)는 표시만, 분기 결정은 매니저가 직접
        PlayerHealth.OnDeath  += OnPlayerDeath;
        _countdown.OnComplete += OnCountdownComplete;
        InitGame();
    }

    void OnDestroy()
    {
        PlayerHealth.OnDeath  -= OnPlayerDeath;
        _countdown.OnComplete -= OnCountdownComplete;
        // Continue 대기 상태에서 씬이 죽으면 InputSystem 구독이 남아있을 수 있음 — 누수 방지
        _anyPress?.Dispose();
    }

    void InitGame()
    {
        // Reset 호출은 InitGame 한 곳에서만 — 게임오버 시 별도 Reset 호출 제거 (책임 단일화)
        _gameState.Reset();
        // 플레이어 초기화 진입점 일원화 — Loader 자동 Start 대신 Manager가 Reset 직후 호출(순서 보장)
        _playerLoader.Load();
        InitCompanions();
    }

    void InitCompanions()
    {
        // 선택된 캐릭터를 제외한 나머지 4종을 동료 슬롯에 채움
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
        // 즉시 입력/스폰 정지 — 사망 시점부터 새 적/입력 차단
        _playerController.SetInputEnabled(false);
        _spawner.Pause();
        _scoreTracker.Pause();
        StartCoroutine(DeathSequence());
    }

    // 사망 흐름 단일 진입점. ResultUI에서 떨어져 나온 분기 로직이 여기로 흡수됨.
    IEnumerator DeathSequence()
    {
        // 결과 패널 페이드인 완료까지 대기 (Show가 IEnumerator)
        yield return _resultUI.Show(_gameState.distanceScore, _gameState.coinCount);

        // coin 유무 관계없이 카운트다운 UI는 켬 — 기획상 게임오버에도 10초 카운트다운 표시
        _countdown.StartCountdown();

        if (_gameState.coinCount > 0)
        {
            // 디바이스 무관 1회 입력 받기 — legacy Input.anyKeyDown 대체
            _anyPress = InputSystem.onAnyButtonPress.Call(_ => TryContinue());
        }
        else
        {
            // coin==0이면 입력 안 받고 페이드만 진행. 카운트다운은 시각적 표시용으로 함께 굴러감
            StartCoroutine(GameOverFade());
        }
    }

    void TryContinue()
    {
        // 입력 1회만 처리하고 구독 해제 — 중복 트리거 방지
        _anyPress?.Dispose();
        _anyPress = null;

        _countdown.StopCountdown();
        _gameState.coinCount--;
        _playerHealth.Revive();
        _spawner.Resume();
        _scoreTracker.Resume();
        _playerController.SetInputEnabled(true);
        _resultUI.Hide();
    }

    void OnCountdownComplete()
    {
        // coin==0은 이미 DeathSequence에서 GameOverFade를 시작했음 — 중복 호출 방지 가드
        if (_gameState.coinCount <= 0) return;

        _anyPress?.Dispose();
        _anyPress = null;
        StartCoroutine(GameOverFade());
    }

    IEnumerator GameOverFade()
    {
        // 페이드 진행 중 입력은 이미 끊겨 있음 — 별도 가드 불필요
        yield return _fadeOverlay.Fade(0f, 1f, _gameOverFadeDuration);
        // GameState.Reset()은 다음 Game 진입 시 InitGame이 수행 — 한 곳에 책임 모음
        SceneFlow.Load(SceneId.Start);
    }
}
