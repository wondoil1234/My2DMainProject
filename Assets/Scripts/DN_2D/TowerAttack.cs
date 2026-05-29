using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackRange = 5f;       // 공격 사거리
    public float attackCooldown = 1f;    // 공격 속도 (쿨타임)
    public LayerMask monsterLayer;       // 몬스터 감지용 레이어

    [Header("화살 발사 세팅")]
    public GameObject arrowPrefab;       // ★ 유니티 에디터에서 연결할 완제품 화살 프리팹
    public Transform firePoint;          // ★ 화살이 스폰될 위치 (비워두면 타워 중심에서 발사)

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
                // ★ 화살이 쫓아갈 수 있도록 찾은 몬스터(targetMonster)를 공격 함수에 넘겨줍니다!
                Attack(targetMonster);
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

    // ★ 화살 소환을 위해 매개변수로 타겟 몬스터를 받도록 수정했습니다.
    void Attack(Transform target)
    {
        Debug.Log("사거리 내 몬스터 공격 및 화살 발사!");

        // 1. 애니메이션 트리거 발동
        if (archerAnimator != null)
        {
            archerAnimator.SetTrigger("isShooting");
        }

        // 2. [핵심] 화살 생성 및 발사 처리
        if (arrowPrefab != null && target != null)
        {
            // 화살이 태어날 위치 결정 (firePoint가 없으면 타워 중심)
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

            // 화살 프리팹을 실시간으로 복사하여 월드에 소환
            GameObject arrowObject = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

            // 소환된 화살에서 우리가 짠 'Arrow' 스크립트를 조각냅니다.
            Arrow arrowScript = arrowObject.GetComponent<Arrow>();

            if (arrowScript != null)
            {
                // 화살에게 방금 조준한 몬스터 타겟을 지정해 줍니다!
                arrowScript.Setup(target);
            }
        }
    }

    // 에디터 씬 뷰에서 사거리를 시각적으로 확인하기 위한 코드
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}