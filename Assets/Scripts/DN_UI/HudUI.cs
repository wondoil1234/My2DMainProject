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

        var slotComponent = Gobj.GetComponent<HudSlotUI>();
        if (slotComponent == null) return;

        slotComponent.InitSlot(instanceId, targettransform);
        _hudslotList.Add(instanceId, slotComponent);
    }

    public void RemoveHudSlot(int instanceId)
    {
        if( _hudslotList.ContainsKey(instanceId) == true)
        {
            var slot = _hudslotList[instanceId]; 

            Destroy(slot.gameObject);

            _hudslotList.Remove(instanceId);
        }
    }

    public void ClearAllSlots()
    {
        foreach (var slot in _hudslotList.Values)
        {
            if (slot != null && slot.gameObject != null)
            {
                Destroy(slot.gameObject);
            }
        }

        _hudslotList.Clear();
        Debug.Log("[HudUI] 원본 데이터 보존 완료! 동적 체력바 슬롯만 안전하게 청소했습니다.");
    }
}
