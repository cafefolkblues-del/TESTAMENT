using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyData[] _enemyPool;
    [SerializeField] EnemyData   _giantData;
    [SerializeField] GameState   _gameState;
    [SerializeField] Transform   _spawnPoint;
    [SerializeField] Enemy       _enemyPrefab;   // 제너릭 적 프리팹 — EnemyData로 구성
    [SerializeField] float       _spawnInterval = 2f;

    Coroutine _spawnCoroutine;

    void Start() => Resume();

    public void Pause()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;
    }

    public void Resume()
    {
        _spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            SpawnEnemy(PickEnemy());
            _gameState.waveCount++;
        }
    }

    EnemyData PickEnemy()
    {
        if (_enemyPool == null || _enemyPool.Length == 0) return null;
        if (_gameState.waveCount > 0 && _gameState.waveCount % 3 == 0) return _giantData;
        return WeightedRandom(_enemyPool);
    }

    EnemyData WeightedRandom(EnemyData[] pool)
    {
        float total = 0f;
        foreach (var e in pool) total += e.spawnWeight;

        float roll       = Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var e in pool)
        {
            cumulative += e.spawnWeight;
            if (roll <= cumulative) return e;
        }
        return pool[pool.Length - 1];
    }

    void SpawnEnemy(EnemyData data)
    {
        if (data == null || _enemyPrefab == null) return;
        // 제너릭 프리팹 1종을 EnemyData로 구성 (Instantiate 직후 Init). 풀링은 후순위.
        Instantiate(_enemyPrefab, _spawnPoint.position, Quaternion.identity).Init(data);
    }
}
