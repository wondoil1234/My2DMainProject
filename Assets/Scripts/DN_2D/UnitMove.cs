using System;
using UnityEngine;

public class UnitMove : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float arrivalDistance = 0.1f;

    [Header("전투 및 체력 설정")]
    public int attackDamage = 1;       // 전사의 공격력
    public float attackCooldown = 1f;  // 공격 주기 (1초에 한 번)

    // ⭐ [체력 바 HUD 연동 변수]
    public int _instanceId;            // 유닛 고유 ID
    public int _baseHp = 50;           // 전사 현재 체력
    private int _maxHp = 50;           // 전사 최대 체력
    public bool _isAlive = true;       // 생존 여부

    private Transform[] _waypoints;
    private int _currentWaypointIndex;
    private bool _isMoving = false;
    private float _currentSpeed;

    // 현재 싸우고 있는 대상의 MonsterMove 스크립트를 보관
    private MonsterMove _targetTargetMonster;
    private float _attackTimer = 0f;

    // 애니메이터 제어용 컴포넌트 변수
    private Animator _animator;

    // ⭐ [체력 바 HUD 연동 이벤트]
    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Start()
    {
        _currentSpeed = moveSpeed;
        _animator = GetComponent<Animator>();

        // 스폰되자마자 HUD 등록
        InitUnitHUD();
    }

    private void InitUnitHUD()
    {
        _instanceId = gameObject.GetInstanceID();
        _maxHp = _baseHp;

        var hudUIBase = DaniTechUIManager.Instance.GetOpendUI(DaniTechUIRootType.MainUI, DaniTechUIType.HudUI);

        if (hudUIBase != null)
        {
            HudUI hudSystem = hudUIBase.GetComponent<HudUI>();
            if (hudSystem != null)
            {
                // 생성된 슬롯이 내 UnitMove를 검사해서 이벤트를 자동으로 바인딩합니다.
                hudSystem.AddHudSlot(_instanceId, this.gameObject.transform);
            }
        }
    }

    // 외부(MonsterMove)에서 전사를 공격할 때 호출할 피격 함수
    public void TakeDamage(int damage)
    {
        if (!_isAlive) return;

        _baseHp -= damage;
        Debug.Log($"<color=yellow>[유닛 피격] 전사가 공격당함! 현재 체력: {_baseHp} / {_maxHp}</color>");

        // 체력 바 실시간 동기화 신호 방출
        _onHpChanged?.Invoke(_baseHp, _maxHp);

        if (_baseHp <= 0)
        {
            OnUnitDie();
        }
    }

    // ⭐ [수정 완료] 체력바 슬롯을 HudUI를 통해 안전하게 지우도록 변경!
    private void OnUnitDie()
    {
        _isAlive = false;
        Debug.Log($"[유닛 사망] {gameObject.name}이 전사했습니다.");

        // 진짜 체력바 담당인 HudUI를 찾아서 삭제 요청을 보냅니다.
        var hudUIBase = DaniTechUIManager.Instance.GetOpendUI(DaniTechUIRootType.MainUI, DaniTechUIType.HudUI);
        if (hudUIBase != null && _instanceId != 0)
        {
            HudUI hudSystem = hudUIBase.GetComponent<HudUI>();
            if (hudSystem != null)
            {
                hudSystem.RemoveHudSlot(_instanceId);
            }
        }

        Destroy(gameObject);
    }

    // UI 매니저/HudSlotUI가 내 체력 이벤트를 묶을 수 있도록 통로 제공
    public void BindOnstatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;

        // 연결되는 순간 꽉 찬 피로 게이지 동기화
        _onHpChanged?.Invoke(_baseHp, _maxHp);
    }

    private void Update()
    {
        if (!_isAlive) return;

        // 1. 전투 중일 때 처리
        if (_targetTargetMonster != null)
        {
            if (_targetTargetMonster == null || _targetTargetMonster._isAlive == false || _targetTargetMonster.gameObject == null)
            {
                _targetTargetMonster = null;
                _currentSpeed = moveSpeed;

                if (_animator != null) _animator.SetBool("isMoving", true);
                Debug.Log($"[전투 종료] 타겟 몬스터가 소멸하여 {gameObject.name}이 다시 전진합니다.");
            }
            else
            {
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= attackCooldown)
                {
                    AttackTarget();
                    _attackTimer = 0f;
                }
                return;
            }
        }

        // 2. 이동 로직
        if (!_isMoving || _waypoints == null || _waypoints.Length == 0)
        {
            if (_animator != null) _animator.SetBool("isMoving", false);
            return;
        }

        if (_currentSpeed == 0f && _targetTargetMonster == null)
        {
            _currentSpeed = moveSpeed;
        }

        if (_animator != null && _currentSpeed > 0f)
        {
            _animator.SetBool("isMoving", true);
        }

        Transform targetWaypoint = _waypoints[_currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.Translate(direction * _currentSpeed * Time.deltaTime);

        if (direction.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);

        if (Vector3.Distance(transform.position, targetWaypoint.position) <= arrivalDistance)
        {
            _currentWaypointIndex--;

            if (_currentWaypointIndex < 0)
            {
                _isMoving = false;
                if (_animator != null) _animator.SetBool("isMoving", false);
                OnReachMonsterSpawnPoint();
            }
        }
    }

    private void AttackTarget()
    {
        if (_targetTargetMonster != null)
        {
            Debug.Log($"[전투] 전사가 슬라임을 공격합니다! 데미지: {attackDamage}");

            if (_animator != null)
            {
                _animator.SetTrigger("doAttack");
            }

            _targetTargetMonster.TakeDamage(attackDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) { CheckAndStartBattle(collision); }
    private void OnTriggerStay2D(Collider2D collision) { if (_targetTargetMonster == null) CheckAndStartBattle(collision); }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            MonsterMove enemy = collision.GetComponent<MonsterMove>();
            if (enemy == _targetTargetMonster || _targetTargetMonster == null)
            {
                _targetTargetMonster = null;
                _currentSpeed = moveSpeed;
                if (_animator != null) _animator.SetBool("isMoving", true);
                Debug.Log($"[시야 해제] 적이 범위 밖으로 벗어나 {gameObject.name}이 다시 이동합니다.");
            }
        }
    }

    private void CheckAndStartBattle(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            MonsterMove enemy = collision.GetComponent<MonsterMove>();

            if (enemy != null && enemy._isAlive)
            {
                _targetTargetMonster = enemy;
                _currentSpeed = 0f;
                _attackTimer = attackCooldown;
                if (_animator != null) _animator.SetBool("isMoving", false);

                Debug.Log($"[전투 발생] 태그(Enemy)를 가진 {collision.gameObject.name}를 발견하여 전투를 시작합니다!");
            }
        }
    }

    public void InitUnitPath(Transform[] allWaypoints)
    {
        if (allWaypoints == null || allWaypoints.Length == 0) return;

        _waypoints = allWaypoints;
        _currentWaypointIndex = _waypoints.Length - 1;
        transform.position = _waypoints[_currentWaypointIndex].position;

        _currentWaypointIndex--;
        _isMoving = true;
        _currentSpeed = moveSpeed;

        if (_animator != null) _animator.SetBool("isMoving", true);
    }

    // ⭐ [수정 완료] 기지에 도달했을 때도 HudUI를 거쳐서 체력바를 지우도록 변경!
    private void OnReachMonsterSpawnPoint()
    {
        Debug.Log($"{gameObject.name}이(가) 몬스터 기지에 도달했습니다!");

        var hudUIBase = DaniTechUIManager.Instance.GetOpendUI(DaniTechUIRootType.MainUI, DaniTechUIType.HudUI);
        if (hudUIBase != null && _instanceId != 0)
        {
            HudUI hudSystem = hudUIBase.GetComponent<HudUI>();
            if (hudSystem != null)
            {
                hudSystem.RemoveHudSlot(_instanceId);
            }
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        _isAlive = false;
        _onHpChanged = null;
    }
}