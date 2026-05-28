using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackRange = 5f;       // 공격 사거리
    public float attackCooldown = 1f;    // 공격 속도 (쿨타임)
    public LayerMask monsterLayer;       // 몬스터 감지용 레이어

    [Header("애니메이터 연결")]
    [SerializeField] private Animator archerAnimator; // 직접 드래그앤드롭할 궁수 애니메이터

    private float nextAttackTime = 0f;

    void Update()
    {
        // 사거리 내에 있는 가장 가까운 몬스터 찾기
        Transform targetMonster = FindTarget();

        if (targetMonster != null)
        {
            // 쿨타임이 지났다면 공격 실행
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    Transform FindTarget()
    {
        // 사거리(attackRange) 내의 모든 몬스터 충돌체를 감지
        Collider2D[] monsters = Physics2D.OverlapCircleAll(transform.position, attackRange, monsterLayer);

        Transform closestMonster = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D monster in monsters)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
            if (distanceToMonster < shortestDistance)
            {
                shortestDistance = distanceToMonster;
                closestMonster = monster.transform;
            }
        }

        return closestMonster;
    }

    void Attack()
    {
        Debug.Log("사거리 내 몬스터 공격!");

        if (archerAnimator != null)
        {
            // 애니메이터에 설정한 Trigger 이름을 발동시킵니다.
            archerAnimator.SetTrigger("isShooting");
        }
    }

    // 에디터 씬 뷰에서 사거리를 시각적으로 확인하기 위한 코드
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}