using System; // Action 사용을 위해 필수 추가
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 3f;            // 이동 속도
    private Transform targetWaypoint;   // 목표 이정표
    private int waypointIndex = 0;      // 이정표 번호

    [Header("전투 및 UI (HUD) 관련 추가 세팅")]
    public int _instanceId;             // 몬스터의 고유 ID (매니저 연동용)
    public int _baseHp = 3;             // 현재 체력 (원하는 만큼 수정 가능)
    private int _maxHp = 3;             // 최대 체력
    public bool _isAlive = true;        // 살아있는지 여부

    // UI 매니저와 통신할 이벤트 변수들
    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    void Start()
    {
        // Collider가 없으므로 Rigidbody 조작 코드는 전부 삭제합니다.
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            targetWaypoint = Waypoints.points[0];
        }
    }

    // ★ [매우 중요] 스폰 매니저가 몬스터를 만들 때 이 함수를 무조건 호출해 줄 겁니다.
    // 기존 GameMonster에 있던 HUD 생성 기능을 이사 왔습니다.
    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _baseHp = monsterData.BaseHp; // 엑셀에서 500 완벽 로드
        }

        _maxHp = _baseHp; // 최대 체력도 500으로 완벽 동기화

        // ----------------------------------------------------------------------
        // ★ [체력 바 새로고침 마법] 
        // UI 매니저에게 내 머리 위에 슬롯을 만들어 달라고 요청한 직후,
        // 반드시 대미지 이벤트를 한 번 리셋하고 다시 갱신해 주어야 UI가 굳지 않고 실시간으로 움직입니다!
        // ----------------------------------------------------------------------
        if (DaniTechUIManager.Instance != null)
        {
            // 1. 혹시 남아있을지 모르는 옛날 연결 고리를 깔끔하게 청소합니다.
            ResetStartChangedEvent();

            // 2. UI 매니저에게 현재 500 피통을 가진 내 몸통 트랜스폼을 넘겨주며 UI 생성을 요청합니다.
            DaniTechUIManager.Instance.AddHudSlot(instanceId, this.gameObject.transform);

            // 3. "나 피 500 들고 태어났어!"라고 UI 전광판에 첫 신호를 쾅 쏴줍니다.
            InvokestatchangedEvent();
        }
    }

    void Update() // ★ 충돌체가 없을 때는 Update에서 등속 이동을 시키는 게 가장 부드럽습니다!
    {
        if (targetWaypoint == null || !_isAlive) return; // 죽었다면 이동 정지

        // 1. 현재 위치에서 목표 이정표까지의 가로세로(X,Y) 방향 계산
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(targetWaypoint.position.x, targetWaypoint.position.y, currentPos.z);
        Vector3 direction = (targetPos - currentPos).normalized;

        // 2. 일정한 속도로 이정표를 향해 정직하게 전진 (벽이 없으므로 Translate가 가장 정확함)
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 3. 좌우 반전 처리
        if (direction.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);

        // 4. [핵심 버그 수정] 오버슈트 방지 판정
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

    // ★ [수정] 기지에 도달해서 사라질 때도 안전하게 HUD를 먼저 지워줍니다.
    void EndPath()
    {
        Debug.Log("몬스터가 기지에 도달했습니다!");
        _isAlive = false;

        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        }

        Destroy(gameObject);
    }

    // =========================================================================
    // 여기서부터 화살 대미지 연산 및 UI 연동을 위해 추가된 핵심 기능들입니다.
    // =========================================================================

    // ★ 화살(Arrow.cs)이 이 함수를 때려 대미지를 주게 됩니다!
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

    // 진짜 사망 처리 함수
    private void OnBattleUnitDie()
    {
        _isAlive = false;

        // 죽을 때 머리 위의 UI 체력 바 슬롯을 지워줍니다.
        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        }

        Destroy(this.gameObject);
    }

    // UI 매니저가 체력 바 시스템을 연결할 때 쓰는 필수 함수 3종 세트
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