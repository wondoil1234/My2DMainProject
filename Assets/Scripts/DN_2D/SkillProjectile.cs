using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SpriteRenderer_Effect;
    [SerializeField] private float ProjectileSpeed = 5.0f;


    private int _damage;
    
    
    private Vector3 _moveDirection = new Vector3(1, 0, 0);
    
    public void InitSkillObject(bool isDirRight, Vector3 playerPos, int damage)
    {
        this.transform.position = playerPos;



        _moveDirection = isDirRight ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
        SpriteRenderer_Effect.flipX = !isDirRight;
        SpriteRenderer_Effect.flipY = !isDirRight;

        _damage = damage;

    }

    private void Update()
    {
        transform.position += _moveDirection * ProjectileSpeed * Time.deltaTime;
    }

   
}
