using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks; // UniTask 사용을 위해 필수!
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // 싱글톤 패턴으로 어디서든 웨이브 정보에 접근할 수 있게 합니다.
    public static WaveManager Inst { get; private set; }

    // 💡 에디터 인스펙터 창에서 깔끔하게 웨이브를 설계할 수 있도록 구조체를 만듭니다.
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

    [Header("[웨이브 설정]")]
    [SerializeField] private WaveData[] _waves;       // 단계별 웨이브 배열
    [SerializeField] private Transform _spawnPoint;   // 몬스터가 태어날 맵의 시작 위치
    [SerializeField] private float _delayBetweenWaves = 10f; // 웨이브 사이의 정비 시간 (초)

    private int _currentWaveIndex = 0;
    private bool _isWaveRunning = false;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        // 게임이 시작되고 3초 뒤에 자동으로 1웨이브를 가동합니다.
        StartNextWaveAsync(3f).Forget();
    }

    /// <summary>
    /// 지정된 대기 시간(초)이 지난 후 다음 웨이브를 실행하는 비동기 함수
    /// </summary>
    public async UniTaskVoid StartNextWaveAsync(float delaySeconds)
    {
        if (_isWaveRunning) return;

        // 모든 웨이브를 다 클리어했다면 종료
        if (_currentWaveIndex >= _waves.Length)
        {
            Debug.Log("★ 축하합니다! 모든 웨이브의 공세를 막아내고 승리했습니다! ★");
            return;
        }

        // 지정된 시간만큼 대기 (초 ➔ 밀리초 변환을 위해 1000을 곱함)
        await UniTask.Delay((int)(delaySeconds * 1000));

        // 실제 몬스터 스폰 웨이브 루프 가동
        SpawnWaveLoopAsync().Forget();
    }

    /// <summary>
    /// 실제 몬스터를 간격에 맞춰 차례대로 소환하는 핵심 루프 함수
    /// </summary>
    private async UniTaskVoid SpawnWaveLoopAsync()
    {
        _isWaveRunning = true;
        WaveData currentWave = _waves[_currentWaveIndex];

        Debug.Log($"=== [{_currentWaveIndex + 1} 웨이브 시작] === 몬스터: {currentWave.monsterDataId} / 총 {currentWave.spawnCount}마리");

        for (int i = 0; i < currentWave.spawnCount; i++)
        {
            // 🚨 [핵심 소환 코드] 우리가 이전에 연동해둔 오브젝트 매니저를 호출하여 
            // 엑셀 데이터를 기반으로 한 진짜 풍뎅이를 실시간으로 맵에 창조합니다.
            if (DaniTechGameObjectManager.Inst != null)
            {
                DaniTechGameObjectManager.Inst.CreatMonsterObject(currentWave.monsterDataId, _spawnPoint).Forget();
            }
            else
            {
                Debug.LogError("DaniTechGameObjectManager 가 씬에 존재하지 않습니다!");
            }

            // 기획한 소환 간격(spawnDelay)만큼 똑똑하게 쉬었다가 다음 마리를 보냅니다.
            await UniTask.Delay((int)(currentWave.spawnDelay * 1000));
        }

        Debug.Log($"[{_currentWaveIndex + 1} 웨이브] 모든 몬스터 스폰 완료!");

        // 다음 웨이브를 위해 인덱스 증가 및 상태 리셋
        _currentWaveIndex++;
        _isWaveRunning = false;

        // 설정된 정비 시간(예: 10초) 뒤에 자동으로 다음 웨이브가 밀려옵니다.
        Debug.Log($"{_delayBetweenWaves}초 후 다음 웨이브가 시작됩니다. 준비하세요!");
        StartNextWaveAsync(_delayBetweenWaves).Forget();
    }
}