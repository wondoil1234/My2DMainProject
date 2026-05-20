using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;


public class GameBookUI : DaniTechUIBase
{
    [Header("동적 생성할 프리팹")]
    [SerializeField] private GameObject Prefab_Slot;

    [Header("디테일 정보 영역")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private Text Text_Descripction;

    [SerializeField] private DaniTechUIButton Button_CloseUI;

    //[Header("부가 정보")]
    //[SerializeField] private GameObject Layout_SubInfoSkill;


    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<string, GameBookSlotUI> _slotList = new Dictionary<string, GameBookSlotUI>();

    private void OnEnable()
    {
        ReadItemListAndCreateSlot();

        Button_CloseUI.BindOnClickButtonEvent(Onclick_CloseGameBookUI);
    }

    public void Onclick_CloseGameBookUI()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.GameBookUI);
    }

    private void OnDisable()
    {
        if(_slotList.Count > 0)
        {
            foreach(var slotkv in _slotList)
            {
                var slot = slotkv.Value;
                DestroyImmediate(slot.gameObject);
            }

            _slotList.Clear();
        }
    }

    private void ReadItemListAndCreateSlot()
    {
        var DatList = DaniTechGameDataManager.Instance.ItemDataList;
        foreach (var datakv in DatList)
        {
            var data = datakv.Value;
            if(data == null) continue;

            CreateGameBookSlot(data.Id);
        }
        
        
        if (_slotList.Count > 0)
        {
            foreach(var slotkv in _slotList)
            {
                var slot = slotkv.Value;
                slot.OnClick_GameBookSlot();
            }
        }
    }





    private void CreateGameBookSlot(string dataId)
    {
        var Gobj = Instantiate(Prefab_Slot, Transform_SlotRoot);
        if (Gobj == null) return;

        var SlotComponent = Gobj.GetComponent<GameBookSlotUI>();
        if (SlotComponent == null) return;

        SlotComponent.InitSlot(dataId, OnClickChildSlotSelected);
        _slotList.Add(dataId, SlotComponent);

    }

    private void OnClickChildSlotSelected(string slotDataId)
    {
        var currentSelectedData = DaniTechGameDataManager.Instance.GetDNItemData(slotDataId);
        if (currentSelectedData == null) return;

        //Image_MainIcon;
        Text_MainName.text = currentSelectedData.Name;
        Text_Descripction.text = currentSelectedData.Description;
        //Text_SellingPrice.text = currentSelectedData.SellingPrice;
        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, currentSelectedData.IconPath).Forget();

        foreach(var slotkv in _slotList)
        {
            var slot = slotkv.Value;
            var dataId = slot.GetSlotDataId();
            slot.SetSelectedUI(slotDataId == dataId);

        }

    }
}
