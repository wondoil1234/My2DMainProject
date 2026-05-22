using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SpriteRenderer_Effect;
    [SerializeField] private float ProjectileSpeed = 5.0f;


    private int _damage;
    private int _ownerInstanceId;
    
    
    private Vector3 _moveDirection = new Vector3(1, 0, 0);
    
    public void InitSkillObject(int ownerInstanceId, bool isDirRight, Vector3 playerPos, int damage)
    {
        this.transform.position = playerPos;



        _moveDirection = isDirRight ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
        SpriteRenderer_Effect.flipX = !isDirRight;
        SpriteRenderer_Effect.flipY = !isDirRight;

        _damage = damage;
        _ownerInstanceId = ownerInstanceId;

    }

    private void Update()
    {
        transform.position += _moveDirection * ProjectileSpeed * Time.deltaTime;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision.collider);
    }

    private void CheckCollision(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var Player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
            Player.TakeDamage(_damage);


            Destroy(this.gameObject);
        }
    }

}
