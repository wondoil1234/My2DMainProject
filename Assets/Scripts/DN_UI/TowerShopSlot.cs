using UnityEngine;
public class TowerShopSlot : DaniTechUIButton
{
    private string towerId;
    private int towerPrice;

    public void InitSlot(string id, Sprite icon, int price)
    {
        this.towerId = id;
        this.towerPrice = price;
        ChangeButtonText(price.ToString());

        UnBindOnClickButtonEvent(OnClickSlot);
        BindOnClickButtonEvent(OnClickSlot);
    }

    public void OnClickSlot()
    {
        if (!GoldManager.Inst.HasGold(towerPrice))
        {
            Debug.Log("골드가 부족합니다!");
            GameObject inventory = GameObject.Find("DNInventory(Clone)");
            if (inventory != null)
                inventory.SetActive(false);

            GameObject towerShop = GameObject.Find("TowerShopPanel");
            if (towerShop != null)
                towerShop.SetActive(false);
            return;
        }

        Debug.Log($"{towerId} 타워 슬롯 클릭됨! 가격은 {towerPrice}");
        TowerPlacer.Inst.StartPlacementFromShop(towerId);
    }
}