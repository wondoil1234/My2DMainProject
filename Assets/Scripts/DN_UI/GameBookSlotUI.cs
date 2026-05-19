using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameBookSlotUI : MonoBehaviour
{
    [Header("슬롯 기본 정보")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private GameObject Gobj_Selected;

    private string _SlotDataId;
    
    
    
    public void InitSlot(string dataId)
    {
        var itemData = DaniTechGameDataManager.Instance.GetDNItemData(dataId);
        if (itemData == null) return;

        Text_MainName.text = itemData.Name;

        string iconpath = itemData.IconPath;
        if(string.IsNullOrEmpty(iconpath) == true) return;

        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconpath).Forget();
        
        _SlotDataId = dataId;

    }

    
}
