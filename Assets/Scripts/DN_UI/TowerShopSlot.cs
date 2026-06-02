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

    }

    public void OnClickSlot()
    { 
        Debug.Log($"{towerId} 타워 슬롯 클릭됨! 가격은 {towerPrice}");
    }
}