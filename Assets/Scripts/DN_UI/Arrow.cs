using UnityEngine;
public class Arrow : MonoBehaviour
{
    [Header("화살 세팅")]
    public float speed = 10f;
    public int damage = 1;
    private int _damage = 1; 

    private Transform target;

    public void SetDamage(int dmg)
    {
        _damage = dmg;
    }

    public void Setup(Transform enemyTarget)
    {
        target = enemyTarget;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 targetPos = new Vector3(
            target.position.x, target.position.y, transform.position.z);
        Vector3 direction = (targetPos - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            MonsterMove monster = other.GetComponent<MonsterMove>();
            if (monster == null)
                monster = other.GetComponentInParent<MonsterMove>();
            if (monster != null)
            {
                monster.TakeDamage(_damage); 
            }
            Destroy(gameObject);
        }
    }
}