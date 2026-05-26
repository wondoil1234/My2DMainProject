using UnityEngine;
using UnityEngine.UI;

public class HudSlotUI : MonoBehaviour
{
    [SerializeField] private int SlotoffsetY;

    [SerializeField] private GameObject LayOut_TextArea;
    [SerializeField] private Text Text_Name;
    [SerializeField] private Slider slider_Hp;
    [SerializeField] private Slider slider_Mp;
    
    private int _instanceId;
    private Transform _targetTransform;

    public void InitSlot(int instanceId, Transform targetTransform)
    {
        _instanceId = instanceId;
        _targetTransform = targetTransform;
        SlotoffsetY = 120;

        TryBindstatChangedEvent(targetTransform.gameObject);
    }

   private void TryBindstatChangedEvent(GameObject gobj)
    {
        var player = gobj.GetComponent<DaniTech_2DPlayer>();
        if (player != null)
        {
            player.BindOnstatChangedEvent(OnTargetEntityHpChanged, OnTargetEntityMpChanged);
            return;
        }
        
        var monster = gobj.GetComponent<GameMonster>();
        if(monster != null)
        {
            monster.BindOnstatChangedEvent(OnTargetEntityHpChanged, OnTargetEntityMpChanged);
            return;
        }
    }

    private void OnTargetEntityHpChanged(int curhp, int maxHp)
    {
        slider_Hp.value = (curhp / (float)maxHp);
    }

    private void OnTargetEntityMpChanged(int curmp, int maxMp)
    {
        slider_Mp.value = (curmp / (float)maxMp);
    }

    private void Update()
    {
        if(_targetTransform != null)
        {
            // this.gameObject.transform.position = _targetTransform.position;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position);

            var rectTransform = this.GetComponent<RectTransform>();
            if(rectTransform != null)
            {
                Vector2 finamScreenPos = new Vector2(screenPos.x, screenPos.y - SlotoffsetY);
                rectTransform.anchoredPosition = finamScreenPos;
            }
        }
    }

}
