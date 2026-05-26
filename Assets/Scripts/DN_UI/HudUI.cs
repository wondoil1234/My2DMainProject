using System.Collections.Generic;
using UnityEngine;

public class HudUI : DaniTechUIBase
{
    [SerializeField] private GameObject prefab_HudSlot;
    [SerializeField] private Transform Transform_SlotRoot;


    private Dictionary<int, HudSlotUI> _hudslotList = new Dictionary<int, HudSlotUI>();


    public void AddHudSlot(int instanceId)
    {
        CreatedHudSlot(instanceId);
    }

    private void CreatedHudSlot(int instanceId)
    {
        var Gobj = Instantiate(prefab_HudSlot, Transform_SlotRoot);
        if (Gobj == null) return;

        var SlotComponent = Gobj.GetComponent<HudSlotUI>();
        if (SlotComponent == null) return;

        //SlotComponent.InitSlot(dataId, OnClickChildSlotSelected);
        _hudslotList.Add(instanceId, SlotComponent);
    }

    public void RemoveHudSlot()
    {

    }
}
