using UnityEngine;
using UnityEngine.UI;

public class TowerSellUI : MonoBehaviour
{
    public static TowerSellUI Inst { get; private set; }

    [Header("판매 금액 비율 (0.5 = 절반 환급)")]
    public float refundRate = 0.5f;

    private TowerClickHandler _selectedTower;
    private Camera _mainCam;

    private void Awake()
    {
        Inst = this;
        _mainCam = Camera.main;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_selectedTower != null)
        {
            Vector3 worldPos = _selectedTower.transform.position;
            worldPos.y += -0.5f;  
            worldPos.x += 1.5f;  
            Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current
                .IsPointerOverGameObject())
            {
                Hide();
            }
        }
    }

    public void Show(TowerClickHandler tower)
    {
        _selectedTower = tower;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _selectedTower = null;
        gameObject.SetActive(false);
    }

    public void OnClickSell()
    {
        if (_selectedTower == null) return;

        int refund = Mathf.RoundToInt(_selectedTower.purchaseCost * refundRate);

        GoldManager.Inst.AddGold(refund);
        Destroy(_selectedTower.gameObject);
        Hide();
    }
}