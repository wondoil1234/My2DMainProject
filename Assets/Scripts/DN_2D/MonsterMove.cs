using System;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 3f;            
    private Transform targetWaypoint;   
    private int waypointIndex = 0;      

    [Header("전투 및 UI (HUD) 관련 추가 세팅")]
    public int _instanceId;            
    public int _baseHp = 3;             
    private int _maxHp = 3;             
    public bool _isAlive = true;       

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    void Start()
    {
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            targetWaypoint = Waypoints.points[0];
        }
    }

    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _baseHp = monsterData.BaseHp; 
        }

        _maxHp = _baseHp; 

        if (DaniTechUIManager.Instance != null)
        {
            ResetStartChangedEvent();

            DaniTechUIManager.Instance.AddHudSlot(instanceId, this.gameObject.transform);

            InvokestatchangedEvent();
        }
    }

    void Update() 
    {
        if (targetWaypoint == null || !_isAlive) return; 

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(targetWaypoint.position.x, targetWaypoint.position.y, currentPos.z);
        Vector3 direction = (targetPos - currentPos).normalized;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (direction.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);

        float distance = Vector2.Distance(currentPos, targetPos);
        if (distance <= 0.2f)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (Waypoints.points == null || waypointIndex >= Waypoints.points.Length - 1)
        {
            EndPath();
            return;
        }

        waypointIndex++;
        targetWaypoint = Waypoints.points[waypointIndex];
    }

    void EndPath()
    {
        Debug.Log("몬스터가 기지에 도달했습니다!");
        _isAlive = false;

        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        }

        if(StageManager.Inst != null)
        {
            StageManager.Inst.DecreaseBaseLife(1);
        }

        Destroy(gameObject);
        CheckVictory();
    }


    public void TakeDamage(int playerdamage)
    {
        if (!_isAlive) return;

        _baseHp -= playerdamage;
        Debug.Log($"[몬스터 피격] 현재 체력: {_baseHp} / {_maxHp}");

        InvokestatchangedEvent();


        if (_baseHp <= 0)
        {
            OnBattleUnitDie();
        }
    }

    private void OnBattleUnitDie()
    {
        _isAlive = false;

        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        }

        Destroy(this.gameObject);
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (WaveManager.Inst == null) return;
        if (!WaveManager.Inst.IsLastWaveDone()) return;
        
        
        
        MonsterMove[] remaining = FindObjectsOfType<MonsterMove>();
        if(remaining.Length <= 1 && WaveManager.Inst.IsLastWaveDone())
        {
            DaniTechGameManager.Inst.TriggerVictory();
        }
    }

    public void BindOnstatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;
    }

    public void ResetStartChangedEvent()
    {
        _onHpChanged = null;
        _onMpChanged = null;
    }

    private void InvokestatchangedEvent()
    {
        _onHpChanged?.Invoke(_baseHp, _maxHp);
    }

    private void OnDisable()
    {
        _isAlive = false;
        ResetStartChangedEvent();
    }
}