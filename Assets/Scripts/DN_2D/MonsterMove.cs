using System;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 3f;
    private Transform targetWaypoint;
    private int waypointIndex = 0;

    [Header("전투 및 UI (HUD) 관련 세팅")]
    public int _instanceId;
    public int _baseHp = 3;
    private int _maxHp = 3;
    public bool _isAlive = true;

    [Header("몬스터 자체 전투 설정")]
    public int monsterDamage = 1;       // 몬스터가 전사를 때리는 공격력
    public float attackCooldown = 1.2f; // 몬스터의 공격 주기
    private float _attackTimer = 0f;
    private float _currentSpeed;

    // 현재 공격 중인 아군 유닛 보관
    private UnitMove _targetUnit;

    // 몬스터 애니메이터 컴포넌트 제어용 변수
    private Animator _animator;

    [Header("골드 설정")]
    public int _rewardGold = 10;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Start()
    {
        _currentSpeed = speed;

        // 내 오브젝트 혹은 자식 오브젝트에 붙어있는 Animator 컴포넌트를 가져옵니다.
        _animator = GetComponentInChildren<Animator>();

        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            targetWaypoint = Waypoints.points[0];
        }
    }

    private void Update()
    {
        if (!_isAlive) return;

        // 1. 전투 처리 (앞에 아군 유닛이 있으면 멈춰서 공격합니다)
        if (_targetUnit != null)
        {
            if (_targetUnit == null || _targetUnit.gameObject == null)
            {
                _targetUnit = null;
                _currentSpeed = speed;
            }
            else
            {
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= attackCooldown)
                {
                    AttackPlayerUnit();
                    _attackTimer = 0f;
                }
                return; // 전투 중일 때는 멈춰 섭니다.
            }
        }

        // 2. 이동 로직 (전투 중이 아닐 때만 목적지를 향해 걷습니다)
        if (targetWaypoint == null) return;

        if (_currentSpeed == 0f && _targetUnit == null)
        {
            _currentSpeed = speed;
        }

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(targetWaypoint.position.x, targetWaypoint.position.y, currentPos.z);
        Vector3 direction = (targetPos - currentPos).normalized;

        transform.Translate(direction * _currentSpeed * Time.deltaTime, Space.World);

        if (direction.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);

        float distance = Vector2.Distance(currentPos, targetPos);
        if (distance <= 0.2f)
        {
            GetNextWaypoint();
        }
    }

    private void AttackPlayerUnit()
    {
        if (_targetUnit != null)
        {
            Debug.Log($"<color=red>[전투] {gameObject.name}이(가) 전사를 공격합니다! 데미지: {monsterDamage}</color>");

            if (_animator != null)
            {
                _animator.SetTrigger("doAttack");
            }

            _targetUnit.TakeDamage(monsterDamage);
        }
    }

    private void HandleCollision(Collider2D collision)
    {
        if (!_isAlive) return;

        // 1️⃣ 아군 전사(PlayerUnit)와 마주쳤을 때: 정지하고 전투 시작!
        if (collision.CompareTag("PlayerUnit"))
        {
            if (_targetUnit == null)
            {
                UnitMove unit = collision.GetComponent<UnitMove>();
                if (unit != null)
                {
                    _targetUnit = unit;
                    _currentSpeed = 0f; // 이동 정지!
                    _attackTimer = attackCooldown - 0.6f;
                    Debug.Log($"[몬스터 전투] {gameObject.name}이 전사를 만나 길을 멈추고 공격을 굳힙니다.");
                }
            }
        }
        // 2️⃣ 정체 및 대기 시스템
        else if (collision.CompareTag("Enemy"))
        {
            MonsterMove frontMonster = collision.GetComponent<MonsterMove>();
            if (frontMonster != null)
            {
                if (frontMonster._currentSpeed == 0f || frontMonster._targetUnit != null)
                {
                    _currentSpeed = 0f;
                }
            }
        }
    }

    // ⭐ [수정 핵심 파트] 오타 교정 및 기지(Finish) 태그 방어선 구축!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 만약 부딪힌 대상이 웨이포인트의 종점(몬스터 기지)이라면
        if (collision.CompareTag("Finish"))
        {
            EndPath();
            return; // 기지에 닿아 끝났으므로 아래 전투 연산은 무시합니다.
        }

        // 기지가 아니라 유닛이나 다른 몬스터라면 원래대로 전투/대기 처리를 합니다.
        HandleCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 기지에 닿은 게 아닐 때만 지속 충돌 체크를 합니다.
        if (!collision.CompareTag("Finish"))
        {
            HandleCollision(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerUnit") || collision.CompareTag("Enemy"))
        {
            if (_targetUnit == null)
            {
                _currentSpeed = speed;
            }
        }
    }

    // --- 이하 기존 매니저 및 시스템 코드 유지 ---
    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null) _baseHp = monsterData.BaseHp;
        _maxHp = _baseHp;

        if (DaniTechUIManager.Instance != null)
        {
            ResetStartChangedEvent();
            DaniTechUIManager.Instance.AddHudSlot(instanceId, this.gameObject.transform);
            InvokestatchangedEvent();
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
        if (DaniTechUIManager.Instance != null) DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        if (StageManager.Inst != null) StageManager.Inst.DecreaseBaseLife(1);
        Destroy(gameObject);
        CheckVictory();
    }

    public void TakeDamage(int playerdamage)
    {
        if (!_isAlive) return;
        _baseHp -= playerdamage;
        Debug.Log($"[몬스터 피격] 현재 체력: {_baseHp} / {_maxHp}");
        InvokestatchangedEvent();

        if (_baseHp <= 0) OnBattleUnitDie();
    }

    private void OnBattleUnitDie()
    {
        _isAlive = false;
        if (DaniTechUIManager.Instance != null) DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        if (GoldManager.Inst != null) GoldManager.Inst.AddGold(_rewardGold);
        Destroy(this.gameObject);
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (WaveManager.Inst == null) return;
        if (!WaveManager.Inst.IsLastWaveDone()) return;

        MonsterMove[] remaining = FindObjectsOfType<MonsterMove>();
        if (remaining.Length <= 1 && WaveManager.Inst.IsLastWaveDone())
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