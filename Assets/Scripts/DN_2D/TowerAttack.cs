using UnityEngine;
public class TowerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    public LayerMask monsterLayer;
    [Header("화살 발사 세팅")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    [Header("애니메이터 연결")]
    [SerializeField] private Animator archerAnimator;
    private float nextAttackTime = 0f;
    private TowerData _towerData;

    private bool _isPlaced = false;

    public void OnPlaced()
    {
        _isPlaced = true;
    }

    private void Start()
    {
        _towerData = GetComponent<TowerData>();
        if (_towerData != null)
        {
            attackRange = _towerData.attackRange;
            attackCooldown = _towerData.attackCooldown;
        }
    }

    void Attack(Transform target)
    {
        Debug.Log("사거리 내 몬스터 공격 및 화살 발사!");
        if (archerAnimator != null)
            archerAnimator.SetTrigger("isShooting");
        if (arrowPrefab != null && target != null)
        {
            Vector3 spawnPosition = firePoint != null ?
                firePoint.position : transform.position;
            GameObject arrowObject = Instantiate(
                arrowPrefab, spawnPosition, Quaternion.identity);
            Arrow arrowScript = arrowObject.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.Setup(target);
                int damage = _towerData != null ? _towerData.damage : 1;
                arrowScript.SetDamage(damage);
            }
        }
    }

    void Update()
    {
        if (!_isPlaced) return;

        Transform targetMonster = FindTarget();
        if (targetMonster != null)
        {
            float distance = Vector3.Distance(transform.position, targetMonster.position);
            if (distance > attackRange)
            {
                targetMonster = null;
            }
        }
        if (targetMonster != null)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack(targetMonster);
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    Transform FindTarget()
    {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}