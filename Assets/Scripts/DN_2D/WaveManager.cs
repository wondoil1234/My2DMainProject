using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("[웨이브 설정]")]
    [SerializeField] private WaveData[] _waves;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _delayBetweenWaves = 10f;

    public static WaveManager Inst { get; private set; }

    [Serializable]
    public struct WaveData
    {
        public string monsterDataId;
        public int spawnCount;
        public float spawnDelay;
    }

    private int _currentWaveIndex = 0;
    private bool _isWaveRunning = false;
    private CancellationTokenSource _cts; // ← 추가

    private void Awake()
    {
        Inst = this;
    }

    public void OnGameStart()
    {
        Debug.Log("게임 시작후 3초후 웨이브가 시작합니다");
        StartNextWaveAsync(3f).Forget();
    }

    public void ResetWave()
    {
        // 기존 UniTask 전부 취소
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _currentWaveIndex = 0;
        _isWaveRunning = false;

        // 몬스터 전부 제거
        MonsterMove[] monsters = FindObjectsOfType<MonsterMove>();
        foreach (MonsterMove monster in monsters)
        {
            Destroy(monster.gameObject);
        }
    }

    public async UniTaskVoid StartNextWaveAsync(float delaySeconds)
    {
        if (_isWaveRunning) return;
        if (_currentWaveIndex >= _waves.Length)
        {
            Debug.Log("★ 모든 웨이브 클리어! ★");
            return;
        }

        var token = _cts.Token;
        await UniTask.Delay((int)(delaySeconds * 1000), cancellationToken: token);
        SpawnWaveLoopAsync().Forget();
    }

    private async UniTaskVoid SpawnWaveLoopAsync()
    {
        _isWaveRunning = true;
        WaveData currentWave = _waves[_currentWaveIndex];
        Debug.Log($"=== [{_currentWaveIndex + 1} 웨이브 시작] ===");

        var token = _cts.Token;

        for (int i = 0; i < currentWave.spawnCount; i++)
        {
            if (token.IsCancellationRequested) return;

            if (DaniTechGameObjectManager.Inst != null)
            {
                DaniTechGameObjectManager.Inst.CreatMonsterObject(
                    currentWave.monsterDataId, _spawnPoint).Forget();
            }
            await UniTask.Delay((int)(currentWave.spawnDelay * 1000),
                cancellationToken: token);
        }

        _currentWaveIndex++;
        _isWaveRunning = false;
        StartNextWaveAsync(_delayBetweenWaves).Forget();
    }
}