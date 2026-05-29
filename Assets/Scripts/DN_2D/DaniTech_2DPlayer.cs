using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System;

// +) 어떤 컴포넌트가 필수로 필요하다는 것을 강제할 수 있다
[RequireComponent(typeof(Rigidbody2D))]
public class DaniTech_2DPlayer : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _jumpForce = 12f;

    [Header("지면 체크 설정")]
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _checkRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)

    [Header("애니메이터")]
    [SerializeField] private DaniTech_2DAnimatorController AnimatorController_Entity;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;
    [SerializeField] private GameObject prefab_SkillProjectile;
    [SerializeField] private Transform Transform_SkillProjectileRoot;

    [Header("전투 관련 정보")]
    [SerializeField] private int _maxHp;
    [SerializeField] private int _PlayerHp = 1000;
    [SerializeField] private int _PlayerBaseAtk = 100;



    // 우선 직접 들고 있다가 추후에 UI매니저한테 요청하도록 개선해볼 것
    [SerializeField] private DaniTech_ScoreUI _scoreUI;

    private Rigidbody2D _rigidBody;
    private bool _isGrounded;
    private float _horizontalInput;
    private bool _lookRight = true;
    private bool _isSkillUsing = false;

    // 추후에는 이런 데이터가 저장될 수 있도록 UI에 있는 것보다 한곳으로 모여지는게 좋다
    private int _currentScore;

    public enum ViewType { SideView, TopDown, Isometric  }
    public ViewType _currentView = ViewType.SideView;
    public Vector2 _lookDirection = Vector2.up;

    private Vector2 _lastOverlapOffset;
    private float _lastOverlapRadius;
    private bool _isOverlapSkillVisible = false;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;


    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        // 2D 캐릭터가 물리 충돌 시 회전해서 넘어지는 것 방지
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        Collider_PlayerNormalAttack.gameObject.SetActive(false);

        _PlayerHp = 1000;
        _maxHp = _PlayerHp;
    }

    private void Start()
    {
        DaniTechGameObjectManager.Inst.RegisterLocalPlayer(this);
        DaniTechUIManager.Instance.AddHudSlot(0, this.gameObject.transform);
        
    }

    private void OnDisable()
    {
        ResetStartChangedEvent();
    }

    void Update()
    {
        // 1. 입력 받기 (Update에서 수행)
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. 점프 입력
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            Jump();
        }

        // 3. 캐릭터 방향 전환 (Flip)
        if (_horizontalInput > 0 && !_lookRight)
        {
            Flip();
        }
        else if (_horizontalInput < 0 && _lookRight) 
        { 
            Flip(); 
        }

        // 이동을 한다라는 판정만 우선 해봅시다
        bool isMoving = (_horizontalInput != 0);
        ChangePlayerState(isMoving ? DaniTech_EntityAnimState.Walk : DaniTech_EntityAnimState.Idle);

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseNormalAttack();
        }

    }

    private void ChangePlayerState(DaniTech_EntityAnimState newState)
    {
        // 이런 곳에 UI나 플레이어의 별도 처리를 넣어줄 수도 있다


        // 우선 애니메이션만 바꿔 봅시다
        AnimatorController_Entity.SetState(newState);
    }

    void FixedUpdate()
    {
        // 4. 지면 체크 (물리 연산 전 수행)
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _checkRadius, _groundLayer);

        // 5. 좌우 이동 처리
        Move();
    }

    void Move()
    {
        // Y축 속도는 유지하면서 X축 속도만 변경 (관성 유지)
        _rigidBody.linearVelocity = new Vector2(_horizontalInput * _moveSpeed, _rigidBody.linearVelocity.y);
    }

    void Jump()
    {
        // 순간적인 힘을 위로 가함
        _rigidBody.linearVelocity = new Vector2(_rigidBody.linearVelocity.x, _jumpForce);
    }

    void Flip()
    {
        _lookRight = !_lookRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // 에디터 뷰에서 지면 체크 범위를 시각적으로 확인


    // 6) 적 충돌 시 처리를 해보자
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false) return;

        // 몸에 부딪힌 몬스터의 컴포넌트를 가져옵니다.
        MonsterMove monster = collision.gameObject.GetComponent<MonsterMove>();
        if (monster == null)
            monster = collision.gameObject.GetComponentInParent<MonsterMove>();

        // 몬스터가 살아있는 상태로 플레이어 몸에 닿았다면 플레이어 피를 깎습니다!
        if (monster != null && monster._isAlive)
        {
            this.TakeDamage(10); // 플레이어 대미지 입음 (원하는 수치로 조정 가능)
            Debug.Log($"[전투] 몬스터가 기지에 침입하여 플레이어가 대미지를 입었습니다!");

            // 만약 디펜스 규칙상 기지에 닿은 몬스터를 자폭(소멸)시키고 싶다면 아래 코드를 활성화하세요.
            // monster.TakeDamage(500); 
        }
    }

    private void AddGameScore()
    {
        // 7) 여기서 맥락 -> UI를 갱신해주기 위해 과연 플레이어가 이렇게 UI를 직접
            // 알고 있는게 좋은걸까?

        _currentScore++;
        _scoreUI.AddGameScore(_currentScore);
    }

    public bool CheckSillUseable(bool isShowMsg = true)
    {
        if(_isSkillUsing == true)
        {
            if(isShowMsg == true)
            {
                DaniTechUIManager.Instance.OpenSimplePopup("스킬이 이미 사용중입니다");
            }
            return false;
        }

        return true;
    }

    public void UseNormalAttack()
    {
        if (CheckSillUseable(isShowMsg:false) == false) return;
      
        ChangePlayerState(DaniTech_EntityAnimState.Atk);
        Collider_PlayerNormalAttack.gameObject.SetActive(true);
        StartCoroutine(CostartNormalAttack());
    }

    public void UseFirstskill()
    {
        if (CheckSillUseable() == false) return;
       
        
        UseOverlapSkill(new Vector2(5.0f, 0.0f), 3.0f);

    }

    

    public void UseSecondskill()
    {
        if (CheckSillUseable() == false) return;

    }

    public void UseThirdskill()
    {
        if (CheckSillUseable() == false) return;
        CreateProjectileSkillObject();

    }

    private void CreateProjectileSkillObject()
    {
        var gObj = Instantiate(prefab_SkillProjectile, Transform_SkillProjectileRoot);
        if (gObj == null) return;
        var skillProjectileComponent = gObj.GetComponent<SkillProjectile>();
        if(skillProjectileComponent == null) return;

        var tag = this.gameObject.tag;
        skillProjectileComponent.InitSkillObject(0,_lookRight, this.transform.position, 500, tag, OnMonsterCollied);
    }

    private void OnMonsterCollied(int monsterinstanceId, int skillDamage)
    {
        var monsterComponent = DaniTechGameObjectManager.Inst.GetMonsterObjectByInstanceId(monsterinstanceId);
        if (monsterComponent == null) return;

        Debug.LogWarning($"플레이어가 {monsterinstanceId}에 데미지 {skillDamage} 부여");
        monsterComponent.TakeDamage(skillDamage);
    }

    IEnumerator CostartNormalAttack()
    {
        _isSkillUsing = true;
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
        _isSkillUsing = false;
    }

    private Vector2 GetAdjustedDirection(Vector2 rawDir)
    {
        switch (_currentView)
        {
            case ViewType.Isometric:
                return new Vector2(rawDir.x - rawDir.y, (rawDir.x + rawDir.y) * 0.5f).normalized;

            case ViewType.SideView:
                return new Vector2(rawDir.x, 0).normalized;

            case ViewType.TopDown:
            default:
                return rawDir.normalized;
        }
    }

    public void UseOverlapSkill(Vector2 offsetPosition, float radius)
    {
        _lastOverlapOffset = offsetPosition;
        _lastOverlapRadius = radius;

        Vector2 adjustedDir = GetAdjustedDirection(_lookDirection);
        Vector2 center = (Vector2)transform.position + new Vector2(adjustedDir.x * offsetPosition.x, adjustedDir.y * offsetPosition.y);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);

        foreach (Collider2D col in hitColliders)
        {
            if (col != null && col.gameObject != this.gameObject)
            {
                Debug.Log($"오버랩 스킬 적중: {col.name}");
            }
        }
    }
    
    
    public void TakeDamage(int damage)
    {
        _PlayerHp -= damage;
        Debug.Log($"{_PlayerHp}");

        InvokestatchangedEvent();
        if (_PlayerHp  < 0)
        {
            PlayerDie();
            DaniTechUIManager.Instance.RemoveHudSlot(0);
        }
    }
    
    public void PlayerDie()
    {
        //bool _isAlive = false;
    }
    
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
        _onHpChanged?.Invoke(_PlayerHp, _maxHp);
        //_onMpChanged?.Invoke(_PlayerMp);
    }
    
    
    
    
    private void OnDrawGizmos()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _checkRadius);
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // 반투명 노란색
        Vector2 adjustedDir = GetAdjustedDirection(_lookDirection);
        Vector3 center = transform.position + new Vector3(adjustedDir.x * _lastOverlapOffset.x, adjustedDir.y * _lastOverlapOffset.y, 0);
        Gizmos.DrawWireSphere(center, _lastOverlapRadius);
    }


    public void AddHp(int hp)
    {
        _PlayerHp += hp;
        InvokestatchangedEvent();
    }

    public void AddAtk(int atk)
    {
        _PlayerBaseAtk += atk;
    }


}
