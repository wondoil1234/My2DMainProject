using UnityEngine;

public class GameMonster : MonsterBase
{
    [Header("데이터를 확인할 수 있도록 임시로 열어줌")]
    public int _instanceId;
    public string _dataId;

    [Header("받아왔는데 전투에서 필요한 데이터")]
    private DNMonsterData _thisMonsterData;
    public int _baseHp;
    public int _baseAtk;

    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;

        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
        if(monsterData != null )
        {
            _thisMonsterData = monsterData;
            _baseHp = _thisMonsterData.BaseHp;
            _baseAtk = _thisMonsterData.BaseAtk;
        }
    }

    private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    {
        return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    }

    private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    {
        return (int)(baseAtk *  skillMultiple);
    }
}
