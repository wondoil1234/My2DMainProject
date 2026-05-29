using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 3f;            // 이동 속도
    private Transform targetWaypoint;   // 목표 이정표
    private int waypointIndex = 0;      // 이정표 번호

    void Start()
    {
        // Collider가 없으므로 Rigidbody 조작 코드는 전부 삭제합니다.
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            targetWaypoint = Waypoints.points[0];
        }
    }

    void Update() // ★ 충돌체가 없을 때는 Update에서 등속 이동을 시키는 게 가장 부드럽습니다!
    {
        if (targetWaypoint == null) return;

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
        // 거리가 0.2f 이하로 좁혀지거나, 혹은 이번 프레임에 이정표를 스쳐 지나갔다면 무조건 도착으로 인정!
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
        Destroy(gameObject);
    }
}