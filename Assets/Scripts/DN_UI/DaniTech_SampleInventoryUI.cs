using System.Collections.Generic;
using UnityEngine;

// 관리주체 역할
public class DaniTech_SampleInventoryUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private DaniTechUIButton Button_UseSelectItem;
    [SerializeField] private DaniTechUIButton Button_CloseSelf;
    [SerializeField] private DaniTechUIButton Button_CloseSelfAllArea;

    private Dictionary<long, DaniTech_SampleInventorySlotUI> _itemSlotList = new Dictionary<long, DaniTech_SampleInventorySlotUI>();
    private long _currentSelectedItemUniqueId;

    private void OnEnable()
    {
        Button_UseSelectItem.BindOnClickButtonEvent(OnClick_UseSelectItem, true);
        Button_CloseSelf.BindOnClickButtonEvent(OnClick_ClosePopup);
        Button_CloseSelfAllArea.BindOnClickButtonEvent(OnClick_ClosePopup);
        SetInventoryItemSlotOnEnable();

        ActiveUseSelectItemButton(false);
    }

    private void OnDisable()
    {
        Button_UseSelectItem.UnBindOnClickButtonEvent(OnClick_UseSelectItem);
    }


    private void SetInventoryItemSlotOnEnable()
    {
        // 슬롯 정리 - 혹시 오픈 시점에 다른 슬롯들이 있다면 제거하자
        if(_itemSlotList.Count > 0)
        {
            foreach(var slot in _itemSlotList){
                DestroyImmediate(slot.Value.gameObject);
            }
            _itemSlotList.Clear();
        }

        //인벤오픈 1-1) 인벤토리가 열릴때 플레이어가 보유한 모든 아이템을 출력하는 로직을 넣어봅시다
        var itemList = DaniTechGameManager.Inst.GetPlayerItemList();
        if(itemList == null || itemList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다!");
            return;
        }

        foreach (var itemModel in itemList)
        {
            CreateSlot(itemModel.ItemUniqueId, itemModel.ItemDataId, itemModel.ItemStackCount);
        }
    }


   

    public void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNInventory);
    }


    public void OnClick_UseSelectItem()
    {
        RequestSelectedUseItem();
    }

    private void RequestSelectedUseItem()
    {
        bool isItemRemoved = DaniTechGameManager.Inst.RequestUseItem(_currentSelectedItemUniqueId);
        if(isItemRemoved == true)
        {
            RemoveItemSlot(_currentSelectedItemUniqueId);
            _currentSelectedItemUniqueId = 0;
            ActiveUseSelectItemButton(false);
        } 
    }

    private void ActiveUseSelectItemButton(bool isActive)
    {
        Button_UseSelectItem.gameObject.SetActive(isActive);
    }

    private void RemoveItemSlot(long removeditemUniqueId)
    {
        if(_itemSlotList.ContainsKey(removeditemUniqueId) == false)
        {
            Debug.LogError("이상합니다! 제거가 된 아이템 슬롯을 찾을수가 없네요!");
            return;
        }

        var slotComponent = _itemSlotList[removeditemUniqueId];
        _itemSlotList.Remove(removeditemUniqueId);
        Destroy(slotComponent.gameObject);
    }



    private void CreateSlot(long itemUniqueId, string itemDataId, int itemStackCount)
    {
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        var slotComponent = gObj.GetComponent<DaniTech_SampleInventorySlotUI>();
        if(slotComponent == null) return;


        slotComponent.InitSlot(itemUniqueId, itemDataId, itemStackCount);
        slotComponent.gameObject.name = $"ItemSlot : {slotComponent.SlotItemUniqueId}";

        _itemSlotList.Add(slotComponent.SlotItemUniqueId, slotComponent);

        slotComponent.BindSlotSelectEvent(OnChildSlotSelected);

        if (string.IsNullOrEmpty(itemDataId) == false && itemDataId.StartsWith("Item_Tower_"))
        {
            int towerPrice = 0;

            if (TowerPlacer.Inst != null)
            {
                if (itemDataId == "Item_Tower_1") towerPrice = TowerPlacer.Inst.towerCosts[0];     
                else if (itemDataId == "Item_Tower_2") towerPrice = TowerPlacer.Inst.towerCosts[1]; 
                else if (itemDataId == "Item_Tower_3") towerPrice = TowerPlacer.Inst.towerCosts[2];
                else if (itemDataId == "Item_Tower_4") towerPrice = TowerPlacer.Inst.towerCosts[3]; 
                else if (itemDataId == "Item_Tower_5") towerPrice = TowerPlacer.Inst.towerCosts[4]; 
            }

            slotComponent.SetTowerPriceText(towerPrice);
        }

        if (string.IsNullOrEmpty(itemDataId) == false && itemDataId == "Item_Unit_1")
        {
            var itemData = DaniTechGameDataManager.Instance.GetDNItemData(itemDataId);
            if (itemData != null)
            {
                int unitPrice = int.Parse(itemData.SellingPrice);
                slotComponent.SetTowerPriceText(unitPrice);
            }
        }
    }


    private void OnChildSlotSelected(long selectedItemUniqueId)
    {
        foreach (var slotKv in _itemSlotList)
        {
            var slot = slotKv.Value;
            bool isSlotSelected = (selectedItemUniqueId == slot.SlotItemUniqueId);
            slot.ChangeSelectedState(isSlotSelected);

            if (isSlotSelected == true)
            {
                _currentSelectedItemUniqueId = slot.SlotItemUniqueId;

                if (slot.IsUsableItem == true)
                {
                    var itemList = DaniTechGameManager.Inst.GetPlayerItemList();
                    string targetDataId = "";
                    foreach (var item in itemList)
                    {
                        if (item.ItemUniqueId == selectedItemUniqueId)
                        {
                            targetDataId = item.ItemDataId;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(targetDataId) == false && targetDataId.StartsWith("Item_Tower_"))
                    {
                        string towerPrefabName = "";

                        if (targetDataId == "Item_Tower_1") towerPrefabName = "Tower_Black";
                        else if (targetDataId == "Item_Tower_2") towerPrefabName = "Tower_Blue";
                        else if (targetDataId == "Item_Tower_3") towerPrefabName = "Tower_Purple";
                        else if (targetDataId == "Item_Tower_4") towerPrefabName = "Tower_Yellow"; 
                        else if (targetDataId == "Item_Tower_5") towerPrefabName = "Tower_Red";    

                        if (string.IsNullOrEmpty(towerPrefabName) == false)
                        {
                            DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNInventory);

                            if (TowerPlacer.Inst != null)
                            {
                                TowerPlacer.Inst.StartPlacementFromShop(towerPrefabName);
                            }
                            else
                            {
                                Debug.LogError("화면에 TowerPlacer 스크립트(오브젝트)가 존재하지 않습니다!");
                            }

                            return;
                        }
                    }

                    else if (string.IsNullOrEmpty(targetDataId) == false && targetDataId == "Item_Unit_1")
                    {
                        var itemData = DaniTechGameDataManager.Instance.GetDNItemData(targetDataId);
                        int cost = int.Parse(itemData.SellingPrice);

                        if (!GoldManager.Inst.HasGold(cost))
                        {
                            Debug.Log("골드가 부족합니다!");
                            return;
                        }

                        GoldManager.Inst.SpendGold(cost);
                        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNInventory);
                        string unitPrefabName = "Unit_Warrior";
                        DaniTechGameObjectManager.Inst.CreateUnitObject(unitPrefabName).Forget();
                        return;
                    }
                }

                ActiveUseSelectItemButton(slot.IsUsableItem);
            }
        }

        Debug.LogWarning($"자식 슬롯 {selectedItemUniqueId} 선택됨!");
    }
}
