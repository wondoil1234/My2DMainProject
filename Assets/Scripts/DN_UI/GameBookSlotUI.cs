using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameBookSlotUI : MonoBehaviour
{
    [Header("슬롯 기본 정보")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private GameObject Gobj_Selected;
    [SerializeField] private DaniTechUIButton Button_SlotClick;

    private event Action<string> _onclickSlot;

    private string _SlotDataId;

    private void OnEnable()
    {
        Button_SlotClick.BindOnClickButtonEvent(OnClick_GameBookSlot);
    }

    public void OnClick_GameBookSlot()
    {
        _onclickSlot?.Invoke(_SlotDataId);
    }

    private void OnDisable()
    {
        _onclickSlot = null;
    }


    public void InitSlot(string dataId, Action<string> onClickcallback) 
    {
        var itemData = DaniTechGameDataManager.Instance.GetDNItemData(dataId);
        if (itemData == null) return;

        Text_MainName.text = itemData.Name;

        string iconpath = itemData.IconPath;
        if(string.IsNullOrEmpty(iconpath) == true) return;

        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconpath).Forget();
        
        _SlotDataId = dataId;

        _onclickSlot += onClickcallback;
    }

    


}
