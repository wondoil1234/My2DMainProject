using UnityEngine;


public class Arrow : MonoBehaviour
{
    [Header("화살 세팅")]
    public float speed = 10f;          // 화살이 날아가는 속도
    public int damage = 1;             // 화살이 주는 데미지 양

    private Transform target;          // 궁수가 지정해 준 목표 몬스터 (풍뎅이)

    // ★ [핵심] 궁수가 화살을 쏠 때 이 함수를 호출해서 몬스터 타겟을 쥐여줄 겁니다.
    public void Setup(Transform enemyTarget)
    {
        target = enemyTarget;
    }

    void Update()
    {
        // 만약 날아가는 도중에 몬스터가 이미 죽어서 사라졌다면, 화살도 허공에서 스스로 소환 해제합니다.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. 몬스터의 현재 위치를 향해 방향 계산
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 direction = (targetPos - transform.position).normalized;

        // 2. 목표를 향해 정직하게 일정한 속도로 전진
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 3. 화살촉이 날아가는 방향(몬스터)을 자연스럽게 바라보도록 회전(Z축) 연산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ★ [피격 판정] 화살의 Is Trigger 콜라이더가 무언가와 부딪혔을 때 유니티가 자동으로 실행하는 함수
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameMonster monster = other.GetComponentInParent<GameMonster>();

            if (monster != null)
            {
                monster.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}