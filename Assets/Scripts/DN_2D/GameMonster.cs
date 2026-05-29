using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameMonster : MonsterBase
{
    [Header("몬스터 프리팹에서 미리 세팅할 데이터")]
    public float SkillCoolTime;
    public GameObject Prefab_ThisMonsterSkillObject;
    [SerializeField] private SpriteRenderer SpriteRenderer_Monster;

    [Header("데이터를 확인할 수 있도록 임시로 열어줌")]
    public int _instanceId;
    public string _dataId;

    [Header("받아왔는데 전투에서 필요한 데이터")]
    private DNMonsterData _thisMonsterData;
    public int _baseHp;
    public int _baseAtk;
    public bool _isAlive = true;
    private bool _lookRight = true;
    private int _maxHp;

    private Vector3 _moveDirection;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void OnDisable()
    {
        _isAlive = false;
        ResetStartChangedEvent();
    }

    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if (monsterData != null)
        {
            _thisMonsterData = monsterData;
            _baseHp = _thisMonsterData.BaseHp;
            _maxHp = _baseHp;
            _baseAtk = _thisMonsterData.BaseAtk;
        }

        DaniTechUIManager.Instance.AddHudSlot(instanceId, this.gameObject.transform);

        // ★ [보안] 태어나자마자 머리 위 UI 체력 바를 100% 상태로 갱신해 줍니다.
        InvokestatchangedEvent();

        StartCoroutine(CheckAndUseSkill());
    }

    public int GetMonsterInstanceId()
    {
        return _instanceId;
    }

    private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    {
        return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    }

    private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    {
        return (int)(baseAtk * skillMultiple);
    }

    IEnumerator CheckAndUseSkill()
    {
        while (_isAlive)
        {
            yield return new WaitForSeconds(SkillCoolTime);

            if (_isAlive == false)
            {
                break;
            }

            changeMonsterDirection();
            UseSkill();
        }
    }

    void changeMonsterDirection()
    {
        _lookRight = !_lookRight;
        _moveDirection = new Vector3(_lookRight ? 1 : -1, 0, 0);
        SetMeshDirectionByMoveDirection((int)_moveDirection.x);
    }

    void SetMeshDirectionByMoveDirection(int x)
    {
        if (SpriteRenderer_Monster != null)
        {
            SpriteRenderer_Monster.flipX = (x < 0);
        }
    }

    public void UseSkill()
    {
        if (Prefab_ThisMonsterSkillObject == null || _thisMonsterData == null) return;

        var gObj = Instantiate(Prefab_ThisMonsterSkillObject, DaniTechGameObjectManager.Inst.transform);
        if (gObj == null) return;
        var skillProjectileComponent = gObj.GetComponent<SkillProjectile>();
        if (skillProjectileComponent == null) return;

        float skillMultiple = _thisMonsterData.SkillAtkMultipleList.Count > 0 ? _thisMonsterData.SkillAtkMultipleList[0] : 0;
        int finalSkillDamage = GetFinalSkillDamage(_baseAtk, skillMultiple);
        var tag = this.gameObject.tag;
        skillProjectileComponent.InitSkillObject(_instanceId, _lookRight, this.transform.position, finalSkillDamage, tag, onSkillCollision);
    }

    private void onSkillCollision(int colliedObjectinstanceId, int damage)
    {
        if (colliedObjectinstanceId == 0)
        {
            var Player = DaniTechGameObjectManager.Inst.GetLocalPlayer();
            if (Player != null)
            {
                Player.TakeDamage(damage);
            }
        }
    }

    // ★ 화살(Arrow.cs)이 이 함수를 때려 대미지를 주게 됩니다!
    public void TakeDamage(int playerdamage)
    {
        if (!_isAlive) return; // 이미 죽은 몬스터라면 연산 무시

        _baseHp -= playerdamage;

        // 실시간으로 대미지 입은 수치를 머리 위 HP 바에 반영합니다.
        InvokestatchangedEvent();

        // ★ [버그 수정] 피가 정확히 0이 되어도 죽도록 '<= 0' 상태로 안전장치를 고쳤습니다.
        if (_baseHp <= 0)
        {
            OnBattleUnitDie();
        }
    }

    private void OnBattleUnitDie()
    {
        _isAlive = false; // 사망 플래그 가동
        DaniTechUIManager.Instance.RemoveHudSlot(_instanceId);
        Destroy(this.gameObject);
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
        _onHpChanged?.Invoke(_baseHp, _maxHp);
    }
}