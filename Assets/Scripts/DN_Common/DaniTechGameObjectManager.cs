using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameObjectManager : MonoBehaviour
{
    // 생성할 몬스터의 프리팹
    [SerializeField] private GameObject Prefab_Enemy;
    [SerializeField] private Transform Root_Enemy;

    public static DaniTechGameObjectManager Inst { get; set; }

    // 생성된 오브젝트의 키가 됨
    private int _objectInstanceKeyGenerator = 0;

    // 생성된 오브젝트의 생명을 보관
    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();

    // ★ [수정] 이제 GameMonster가 아니라 MonsterMove를 담는 창고로 변경합니다!
    private Dictionary<int, MonsterMove> _MonsterObjectContainer = new Dictionary<int, MonsterMove>();

    private DaniTech_2DPlayer _LocalPlayer;

    private void Awake()
    {
        Inst = this;
    }

    public void RegisterLocalPlayer(DaniTech_2DPlayer LocalPlayer)
    {
        _LocalPlayer = LocalPlayer;
    }

    public DaniTech_2DPlayer GetLocalPlayer()
    {
        if (_LocalPlayer == null)
        {
            Debug.LogError("등록된 플레이어가 없는데! 참조하려고 시도하고 있습니다!");
            return null;
        }

        return _LocalPlayer;
    }

    public void RequestSpawnEnemy()
    {
        if (Prefab_Enemy == null)
        {
            Debug.LogWarning("프리팹이 등록되지 않은 오브젝트 입니다.");
            return;
        }

        var gObj = Instantiate(Prefab_Enemy, Root_Enemy);
        if (gObj == null)
        {
            Debug.LogWarning("생성에 실패한 게임 오브젝트 입니다.");
            return;
        }

        _objectInstanceKeyGenerator++;

        if (_createdGameObjectContainer.ContainsKey(_objectInstanceKeyGenerator) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            return;
        }

        _createdGameObjectContainer.Add(_objectInstanceKeyGenerator, gObj);
        InitGeneratedEntityObject(_objectInstanceKeyGenerator, gObj);

        Debug.Log($"키: {_objectInstanceKeyGenerator}의 객체 {gObj.name}이 호출되었습니다.");
    }

    private void InitGeneratedEntityObject(int generatedId, GameObject gObj)
    {
        DaniTech_2DEnemy gameEntity = gObj.GetComponent<DaniTech_2DEnemy>();
        if (gameEntity == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

        gameEntity.InitEnemyInfo(generatedId);
    }

    public GameObject GetEntityObjectCanBeNull(int instanceId)
    {
        if (_createdGameObjectContainer.ContainsKey(instanceId) == false)
        {
            Debug.LogWarning($"{instanceId}는 존재하지 않습니다.");
            return null;
        }

        return _createdGameObjectContainer[instanceId];
    }

    public void RequestDestroyEntityObject(int instanceId)
    {
        var gObj = GetEntityObjectCanBeNull(instanceId);
        if (gObj == null)
        {
            return;
        }

        _createdGameObjectContainer.Remove(instanceId);
        Destroy(gObj);
    }

    public async UniTaskVoid CreatMonsterObject(string monsterDataId, Transform spawnSpot)
    {
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);
        if (monsterData == null) return;

        var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(monsterData.PrefabPath, Root_Enemy, true);

        if (spawnSpot != null)
        {
            createdObj.transform.position = spawnSpot.position;
        }
        else
        {
            createdObj.transform.position = Vector3.zero;
        }

        AddMonsterObjectOnCreate(createdObj, monsterDataId);
    }

    public async UniTaskVoid CreateUnitObject(string unitPrefabAddress)
    {
        var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(unitPrefabAddress, Root_Enemy, true);

        if (createdObj == null)
        {
            Debug.LogError($"[소환 실패] {unitPrefabAddress} 프리팹을 어드레서블에서 불러오지 못했습니다.");
            return;
        }

        createdObj.transform.position = Vector3.zero;

        AddMonsterObjectOnCreate(createdObj, unitPrefabAddress);
    }

    private void AddMonsterObjectOnCreate(GameObject createdObject, string monsterDataId)
    {
        _objectInstanceKeyGenerator++;
        int generatedInstanceId = _objectInstanceKeyGenerator;

        var unitMoveComponent = createdObject.GetComponent<UnitMove>();
        if (unitMoveComponent != null)
        {
            if (Waypoints.points != null && Waypoints.points.Length > 0)
            {
                unitMoveComponent.InitUnitPath(Waypoints.points);
                Debug.Log($"[유닛 소환 성공] {createdObject.name}이 역방향 웨이포인트 경로를 주입받았습니다.");
            }
            else
            {
                Debug.LogError("Waypoints.points가 비어있습니다! 맵에 웨이포인트 오브젝트가 배치되었는지 확인하세요.");
            }

            return;
        }

        var monsterComponent = createdObject.GetComponent<MonsterMove>();
        if (monsterComponent == null) return;

        _MonsterObjectContainer.Add(generatedInstanceId, monsterComponent);
        monsterComponent.InitMonster(generatedInstanceId, monsterDataId);
    }

    // ★ [수정] 다른 스크립트에서 참조할 수 있도록 반환 타입을 MonsterMove로 변경합니다.
    public MonsterMove GetMonsterObjectByInstanceId(int monsterInstanceId)
    {
        if (_MonsterObjectContainer.ContainsKey(monsterInstanceId) == false)
        {
            Debug.LogError($"{monsterInstanceId} 찾으려는 몬스터가 유효하지 않습니다");
            return null;
        }
        return _MonsterObjectContainer[monsterInstanceId];
    }

    //[필드 오브젝트] ====================================================================================================

    public async UniTaskVoid CreateFieldObject(string fieldObjectDataId, Transform spawnSpot)
    {
        var fieldObject = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObject != null)
        {
            var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(fieldObject.PrefabPath, Root_Enemy, true);
            createdObj.transform.position = spawnSpot.position;
            AddFieldObjectOnCreate(createdObj, fieldObjectDataId);
        }
    }

    private void AddFieldObjectOnCreate(GameObject createdObject, string fieldObjectDataId)
    {
        _objectInstanceKeyGenerator++;
        var generatedInstanceId = _objectInstanceKeyGenerator;
        var fieldObject = createdObject.GetComponent<DaniTech_2DFieldObject>();

        if (fieldObject != null)
        {
            _fieldObjectContainer.Add(generatedInstanceId, fieldObject);
            fieldObject.InitFieldObjectInfoOnCreated(generatedInstanceId, fieldObjectDataId);
        }
    }

    public void RequestDestroyFieldObject(int instanceId)
    {
        var fieldObjectComponent = GetFieldObjectByInstanceId(instanceId);
        if (fieldObjectComponent == null)
        {
            return;
        }

        _fieldObjectContainer.Remove(instanceId);
        Destroy(fieldObjectComponent.gameObject);
    }

    public DaniTech_2DFieldObject GetFieldObjectByInstanceId(int fieldObjectInstanceId)
    {
        if (_fieldObjectContainer.ContainsKey(fieldObjectInstanceId) == false)
        {
            Debug.LogError($"{fieldObjectInstanceId} 찾으려는 몬스터가 유효하지 않습니다");
            return null;
        }

        return _fieldObjectContainer[fieldObjectInstanceId];
    }
}