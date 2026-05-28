using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 3f;
    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 웨이포인트가 세팅되었는지 안전하게 검사
        if (Waypoints.points != null && Waypoints.points.Length > 0)
        {
            targetWaypoint = Waypoints.points[0];
        }
        else
        {
            Debug.LogWarning(gameObject.name + " : 웨이포인트 그룹을 찾을 수 없습니다! WaypointGroup을 확인하세요.");
        }
    }

    void FixedUpdate()
    {
        // 목표가 없거나 리지드바디가 없으면 코드 실행을 중단하여 에러 방지
        if (targetWaypoint == null || rb == null) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = new Vector2(targetWaypoint.position.x, targetWaypoint.position.y);

        // 방향 계산
        Vector2 direction = (targetPos - currentPos).normalized;

        // 물리 이동 실행
        Vector2 newPosition = currentPos + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // 좌우 반전 처리
        if (direction.x > 0.1f) transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(1, 1, 1);

        // [버그 방지 고정] 거리가 가까워졌거나, 맵 밖으로 밀려나서 끼었을 때를 대비해 판정 거리를 0.5f로 널널하게 상향
        float distance = Vector2.Distance(currentPos, targetPos);
        if (distance <= 0.5f)
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