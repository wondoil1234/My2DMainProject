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

    public string GetSlotDataId()
    {
        return _SlotDataId;
    }

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


    public void InitSlot(string dataId, EgameBookCategory curCategory, Action<string> onClickcallback) 
    {
        if(curCategory == EgameBookCategory.ItemCategory)
        {
            var itemData = DaniTechGameDataManager.Instance.GetDNItemData(dataId);
            if (itemData == null) return;

            Text_MainName.text = itemData.Name;

            string iconpath = itemData.IconPath;
            if (string.IsNullOrEmpty(iconpath) == true) return;

            DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconpath).Forget();
        }
        else if(curCategory == EgameBookCategory.MonsterCategory)
        {
            var MonsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
            if (MonsterData == null) return;

            Text_MainName.text = MonsterData.Name;

            string iconpath = MonsterData.IconPath;
            if (string.IsNullOrEmpty(iconpath) == true) return;

            DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconpath).Forget();

        }
        else if(curCategory == EgameBookCategory.HarvestCategory)
        {
            var FieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(dataId);
            if (FieldObjectData == null) return;
            if (FieldObjectData.FieldObjectType != "Harvest") return;

            Text_MainName.text = FieldObjectData.Name;

            string iconpath = FieldObjectData.IconPath;
            if (string.IsNullOrEmpty(iconpath) == true) return;

            DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconpath).Forget();
        }

        _SlotDataId = dataId;

        _onclickSlot += onClickcallback;
    }

    public void SetSelectedUI(bool isSelect)
    {
        Gobj_Selected.SetActive(isSelect);
    }

}
