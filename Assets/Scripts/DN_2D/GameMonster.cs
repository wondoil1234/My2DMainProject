using UnityEngine;

public class GameMonster : MonsterBase
{
    [Header("데이터를 확인할 수 있도록 임시로 열어줌")]
    public int _instanceId;
    public string _dataId;
 
    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;
    }
}
