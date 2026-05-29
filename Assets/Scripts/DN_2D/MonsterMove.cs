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

        // 1. [순서 교정] 제일 먼저 엑셀 데이터 파일부터 수색해서 500 수치를 명확하게 가져옵니다!
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _baseHp = monsterData.BaseHp; // 엑셀에 적힌 500이 정확하게 대입됩니다.
            Debug.Log($"[엑셀 연동 성공] {dataId} 몬스터의 체력 {_baseHp}을 성공적으로 로드했습니다.");
        }
        else
        {
            // 만약 엑셀 파일과 문패 이름이 안 맞아서 데이터를 못 가져왔다면 콘솔창에 이 경고가 뜰 겁니다!
            Debug.LogError($"[엑셀 연동 실패] {dataId}에 해당하는 데이터를 엑셀에서 찾을 수 없습니다! 기본 체력(50)으로 대체합니다.");
            _baseHp = 50;
        }

        // 2. 엑셀에서 500을 완벽하게 주입받은 '후에' 최대 체력을 동기화해 줍니다. (오버플로우 방지)
        _maxHp = _baseHp;

        // UI 매니저에게 머리 위 체력 바(HUD) 생성을 요청합니다.
        if (DaniTechUIManager.Instance != null)
        {
            DaniTechUIManager.Instance.AddHudSlot(instanceId, this.gameObject.transform);
            InvokestatchangedEvent(); // 500 피통 기준으로 UI 바를 가득 채웁니다.
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

        // 대미지 입은 수치를 머리 위 HP 바에 실시간 중계(반영)합니다.
        InvokestatchangedEvent();

        // 피가 0 이하가 되면 죽습니다.
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