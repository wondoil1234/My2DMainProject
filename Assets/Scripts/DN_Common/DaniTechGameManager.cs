using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameManager : MonoBehaviour
{
    public static DaniTechGameManager Inst { get; set; }

    [Header("Life System")]
    public int maxLife = 3;
    private int currentLife;

    [Header("Ui 연결")]
    public GameObject popupGameOver;
    public GameObject popupVictory;
    public GameObject lifeContainer;
    public GameObject wavePanel;
    public GameObject goldPanel;




    private DaniTechPlayerModel _playerModel = new DaniTechPlayerModel();

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        LoadSaveData();

        if (_playerModel != null && _playerModel.ItemList != null)
        {
            _playerModel.ItemList.Clear(); 
        }
        AddItem("Item_Tower_1", 1);
        AddItem("Item_Tower_2", 1);
        AddItem("Item_Tower_3", 1);
        AddItem("Item_Tower_4", 1);
        AddItem("Item_Tower_5", 1);
        AddItem("Item_Unit_1", 1);

        currentLife = maxLife;
        popupVictory.SetActive(false);
        popupGameOver.SetActive(false);

        if (lifeContainer != null) lifeContainer.SetActive(false);
        if (wavePanel != null) wavePanel.SetActive(false);
        if (goldPanel != null) goldPanel.SetActive(false);
    }

    public void SaveData()
    {
        DaniTechNetworkManager.Inst.RequstSaveData(_playerModel);
    }

    public void SaveAndEndGame()
    {
        SaveData();
        Application.Quit();
    }

    private void LoadSaveData()
    {
        _playerModel = DaniTechNetworkManager.Inst.RequstLoadSaveData();
    }

    public void IncreasePlayerExp(int exp)
    {
        // 추후에 한곳에서 관리할 수 있게 익스텐션으로 빼도 된다
        _playerModel.PlayerTotalExp += exp;
    }

    public void AddItem(string itemDataId, int addItemCount)
    {
        // 저장할때 고유값 ID를 부여하기 위해 사용
        long uniqueId = DaniTechGameUtil.GenerateUniqueId();

        // TODO : 우선 쉽게 사용할 수 있도록 중복 처리는 빼두었다. 습득할때마다 아이템이 하나씩 추가되도록 해두고
        // 추후에 중복값은 StackCount가 다 찰때까지 누적해줄 수 있도록 로직을 추가하자
        var newItem = new DaniTechItemModel();
        newItem.ItemUniqueId = uniqueId;
        newItem.ItemDataId = itemDataId;
        newItem.ItemStackCount = addItemCount;

        _playerModel.ItemList.Add(newItem);
        SaveData();
    }

    public bool RequestUseItem(long requestUseTargetItemUniqueId)
    {
        int removeTargetIdx = 0;
        bool isRemoveItemExist = false;
        foreach (var itemModel in _playerModel.ItemList)
        {
            if (itemModel.ItemUniqueId == requestUseTargetItemUniqueId)
            {
                isRemoveItemExist = true;

                string itemDataId = itemModel.ItemDataId;
                var itemData = DaniTechGameDataManager.Instance.GetDNItemData(itemDataId);
                if (string.IsNullOrEmpty(itemData.UseItemType) == false)
                {
                    UseItemFunction(itemData.UseItemType, itemData.UseItemParameterList, itemData); // itemData 추가
                }

                break;
            }

            removeTargetIdx++;
        }

        RequestRemoveItem(isRemoveItemExist, removeTargetIdx);
        return true;
    }


    private void UseItemFunction(string itemUseType, List<string> useItemParamList, DNItemData itemData)
    {
        if (useItemParamList == null || useItemParamList.Count == 0) return; // return 추가

        if (itemUseType == "RandomItemBox")
        {

        }
        else if (itemUseType == "StatChangeAtk")
        {
            if (useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                int statChangeVal = int.Parse(str);
                var playerComponent = GetLocalPlayer();
                playerComponent.AddAtk(statChangeVal);
            }
        }
        else if (itemUseType == "StatChangeHp")
        {
            if (useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                int statChangeVal = int.Parse(str);
                var playerComponent = GetLocalPlayer();
                playerComponent.AddHp(statChangeVal);
            }
        }
        else if (itemUseType == "SummonMonster")
        {
            if (useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                var strArr = str.Split(":");
                if (strArr.Length > 1)
                {
                    string monsterDataId = strArr[0];

                    int cost = int.Parse(itemData.SellingPrice);
                    if (!GoldManager.Inst.HasGold(cost))
                    {
                        Debug.Log("골드가 부족합니다!");
                        return;
                    }
                    GoldManager.Inst.SpendGold(cost);

                    DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNInventory);

                    if (TowerPlacer.Inst != null)
                        TowerPlacer.Inst.StartPlacementFromShop(monsterDataId);
                }
            }
        }
    }

    private bool RequestRemoveItem(bool isRemoveItemExist, int removeTargetIdx)
    {
        if(isRemoveItemExist == true)
        {
            _playerModel.ItemList.RemoveAt(removeTargetIdx);
            SaveData();
            return true;
        }
        return false;
    }

    public List<DaniTechItemModel> GetPlayerItemList()
    {
        // _playerModel이 Private이므로 외부에서 ItemList를 받아올 수 있게 Get함수를 사용한다
        return _playerModel.ItemList;
    }


    public DaniTech_2DPlayer GetLocalPlayer()
    {
        return DaniTechGameObjectManager.Inst.GetLocalPlayer();
    }

    public void LoseLife()
    {
        if (currentLife <= 0) return;
        currentLife--;

        var lifeContainer = FindObjectOfType<LifeContainer>();
        if (lifeContainer != null)
            lifeContainer.UpdateHearts(currentLife);

        if (currentLife <= 0) TriggerGameOver();
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0f;
        popupVictory.SetActive(true);
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f;
        popupGameOver.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        currentLife = maxLife;
        popupVictory.SetActive(false);
        popupGameOver.SetActive(false);

        StageManager.Inst.ResetGame();

       
        if (lifeContainer != null) lifeContainer.SetActive(true);
        if (wavePanel != null) wavePanel.SetActive(true);
        if (goldPanel != null) goldPanel.SetActive(true);

        var lifeContainerComp = lifeContainer?.GetComponent<LifeContainer>();
        if (lifeContainerComp != null)
            lifeContainerComp.InitHearts(maxLife);

        UnitMove[] activeUnits = FindObjectsOfType<UnitMove>();
        foreach (UnitMove unit in activeUnits)
        {
            if (unit != null && unit.gameObject != null)
                Destroy(unit.gameObject);
        }

        MonsterMove[] activeMonsters = FindObjectsOfType<MonsterMove>();
        foreach (MonsterMove monster in activeMonsters)
        {
            if (monster != null && monster.gameObject != null)
            {
                if (DaniTechUIManager.Instance != null && monster._instanceId != 0)
                    DaniTechUIManager.Instance.RemoveHudSlot(monster._instanceId);
                Destroy(monster.gameObject);
            }
        }

        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj != null && obj.name.Contains("Tower_"))
                Destroy(obj);
        }

        WaveManager.Inst.ResetWave();
        WaveManager.Inst.OnGameStart();
        GoldManager.Inst.ResetGold();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentLife = maxLife;
        StageManager.Inst.ResetGame();
        popupVictory.SetActive(false);
        popupGameOver.SetActive(false);

        if (lifeContainer != null) lifeContainer.SetActive(false);
        if (wavePanel != null) wavePanel.SetActive(false);
        if (goldPanel != null) goldPanel.SetActive(false);

        var lifeContainerComp = lifeContainer?.GetComponent<LifeContainer>();
        if (lifeContainerComp != null)
            lifeContainerComp.InitHearts(maxLife);

        UnitMove[] activeUnits = FindObjectsOfType<UnitMove>();
        foreach (UnitMove unit in activeUnits)
        {
            if (unit != null && unit.gameObject != null)
            {
                Destroy(unit.gameObject);
            }
        }

        MonsterMove[] activeMonsters = FindObjectsOfType<MonsterMove>();
        foreach (MonsterMove monster in activeMonsters)
        {
            if (monster != null && monster.gameObject != null)
            {
                if (DaniTechUIManager.Instance != null && monster._instanceId != 0)
                {
                    DaniTechUIManager.Instance.RemoveHudSlot(monster._instanceId);
                }
                Destroy(monster.gameObject);
            }
        }

        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj != null && obj.name.Contains("Tower_"))
            {
                Destroy(obj);
            }
        }

        WaveManager.Inst.ResetWave();
        GoldManager.Inst.ResetGold();

        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.RobbyUI);
    }
}

