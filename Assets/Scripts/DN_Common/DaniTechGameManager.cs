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
    public List<GameObject> heartList;



    private DaniTechPlayerModel _playerModel = new DaniTechPlayerModel();

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        LoadSaveData();

        bool hasTower = false;
        if (_playerModel != null && _playerModel.ItemList != null)
        {
            foreach (var item in _playerModel.ItemList)
            {
                if (item.ItemDataId == "Item_Tower_1")
                {
                    hasTower = true;
                    break;
                }
            }
        }

        if (hasTower == false)
        {
            AddItem("Item_Tower_1", 1);
        }
        currentLife = maxLife;
        popupVictory.SetActive(false);
        popupGameOver.SetActive(false);
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
                if(string.IsNullOrEmpty(itemData.UseItemType) == false)
                {
                    UseItemFunction(itemData.UseItemType, itemData.UseItemParameterList);
                }




                break;
            }

            removeTargetIdx++;
        }

        RequestRemoveItem(isRemoveItemExist, removeTargetIdx);
        return true;
    }

    private void UseItemFunction(string itemUseType, List<string> useItemParamList)
    {
        if (useItemParamList == null || useItemParamList.Count == 0)




        if (itemUseType == "RandomItemBox")
        {

        }
        else if (itemUseType == "StatChangeAtk")
        {
            if(useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                int statChangeVal = int.Parse(str);
                var playerComponent = GetLocalPlayer();
                playerComponent.AddAtk(statChangeVal);
            }

        }
        else if (itemUseType == "StatChangeHp")
        {
            if(useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                int statChangeVal = int.Parse(str);
                var playerComponent = GetLocalPlayer();
                playerComponent.AddHp(statChangeVal);

            }
        }
        else if (itemUseType == "SummonMonster")
        {
            if(useItemParamList.Count > 0)
            {
                string str = useItemParamList[0];
                var strArr = str.Split(":");
                if(strArr.Length > 1)
                {
                    string monsterDataId = strArr[0];
                    int monsterSummonCount = int.Parse(strArr[1]);

                    for(int i = 0; i < monsterSummonCount; i++)
                    {
                        var playerComponent = GetLocalPlayer();
                        DaniTechGameObjectManager.Inst.CreatMonsterObject(monsterDataId, playerComponent.transform).Forget();
                    }


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
        heartList[currentLife].SetActive(false);
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
        StageManager.Inst.ResetGame();
        popupVictory.SetActive(false);
        popupGameOver.SetActive(false);

        for (int i = 0; i < heartList.Count; i++)
            heartList[i].SetActive(true);

        // 웨이브 리셋 후 바로 게임 시작
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

        for (int i = 0; i < heartList.Count; i++)
            heartList[i].SetActive(true);

        // 웨이브 리셋
        WaveManager.Inst.ResetWave();
        GoldManager.Inst.ResetGold();


        // 로비UI 열기
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.RobbyUI);
    }
}

