using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBookUI : DaniTechUIBase
{
    [Header("동적 생성할 프리팹")]
    [SerializeField] private GameObject Prefab_Slot;

    [Header("디테일 정보 영역")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private Text Text_Descripction;

    //[Header("부가 정보")]
    //[SerializeField] private GameObject Layout_SubInfoSkill;


    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<string, GameBookSlotUI> _slotList = new Dictionary<string, GameBookSlotUI>();



    private void CreateGameBookSlot(string dataId)
    {
        var Gobj = Instantiate(Prefab_Slot, Transform_SlotRoot);
        if (Gobj == null) return;

        var SlotComponent = Gobj.GetComponent<GameBookSlotUI>();
        if(SlotComponent == null) return;

        SlotComponent.InitSlot(dataId);
        _slotList.Add(dataId, SlotComponent);

    }
}
