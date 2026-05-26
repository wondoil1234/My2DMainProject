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
        SlotoffsetY = 50;

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
        if (_targetTransform != null)
        {
            Vector3 worldPos = _targetTransform.position;

            worldPos.y -= 0.6f;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0) return;

            this.transform.position = screenPos;
        }
    }

}
