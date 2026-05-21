using UnityEngine;

public class GameMonster : MonsterBase
{

    private int _instanceId;
    private string _dataId;
 
    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;
    }
}
