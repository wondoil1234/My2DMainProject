using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks; 
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
        [Tooltip("엑셀 데이터 시트에 적힌 몬스터의 고유 ID")]
        public string monsterDataId;

        [Tooltip("이번 웨이브에 스폰할 총 몬스터 수")]
        public int spawnCount;

        [Tooltip("몬스터와 몬스터 사이의 출현 간격 (초 단위)")]
        public float spawnDelay;
    }


    private int _currentWaveIndex = 0;
    private bool _isWaveRunning = false;

    private void Awake()
    {
        Inst = this;
    }

    public void OnGameStart()
    {
        Debug.Log("게임 시작후 3초후 웨이브가 시작합니다");
        StartNextWaveAsync(10f).Forget();
    }

    public async UniTaskVoid StartNextWaveAsync(float delaySeconds)
    {
        if (_isWaveRunning) return;

        if (_currentWaveIndex >= _waves.Length)
        {
            Debug.Log("★ 축하합니다! 모든 웨이브의 공세를 막아내고 승리했습니다! ★");
            return;
        }

        await UniTask.Delay((int)(delaySeconds * 1000));

        SpawnWaveLoopAsync().Forget();
    }

    private async UniTaskVoid SpawnWaveLoopAsync()
    {
        _isWaveRunning = true;
        WaveData currentWave = _waves[_currentWaveIndex];

        Debug.Log($"=== [{_currentWaveIndex + 1} 웨이브 시작] === 몬스터: {currentWave.monsterDataId} / 총 {currentWave.spawnCount}마리");

        for (int i = 0; i < currentWave.spawnCount; i++)
        {
            if (DaniTechGameObjectManager.Inst != null)
            {
                DaniTechGameObjectManager.Inst.CreatMonsterObject(currentWave.monsterDataId, _spawnPoint).Forget();
            }
            else
            {
                Debug.LogError("DaniTechGameObjectManager 가 씬에 존재하지 않습니다!");
            }

            await UniTask.Delay((int)(currentWave.spawnDelay * 1000));
        }

        Debug.Log($"[{_currentWaveIndex + 1} 웨이브] 모든 몬스터 스폰 완료!");

        _currentWaveIndex++;
        _isWaveRunning = false;

        Debug.Log($"{_delayBetweenWaves}초 후 다음 웨이브가 시작됩니다. 준비하세요!");
        StartNextWaveAsync(_delayBetweenWaves).Forget();
    }
}