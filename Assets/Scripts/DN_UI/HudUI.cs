using System.Collections.Generic;
using UnityEngine;

public class HudUI : DaniTechUIBase
{
    [SerializeField] private GameObject prefab_HudSlot;
    [SerializeField] private Transform Transform_SlotRoot;


    private Dictionary<int, HudSlotUI> _hudslotList = new Dictionary<int, HudSlotUI>();


    public void AddHudSlot(int instanceId,Transform targettransform)
    {
        CreatedHudSlot(instanceId, targettransform);
    }

    private void CreatedHudSlot(int instanceId, Transform targettransform)
    {
        var Gobj = Instantiate(prefab_HudSlot, Transform_SlotRoot);
        if (Gobj == null) return;

        var SlotComponent = Gobj.GetComponent<HudSlotUI>();
        if (SlotComponent == null) return;

        SlotComponent.InitSlot(instanceId, targettransform);
        _hudslotList.Add(instanceId, SlotComponent);
    }

    public void RemoveHudSlot()
    {

    }
}
