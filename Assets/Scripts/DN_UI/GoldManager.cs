using UnityEngine;
using UnityEngine.UI;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Inst { get; private set; }

    [Header("골드 설정")]
    [SerializeField] private int _startGold = 100;
    private int _currentGold;

    [Header("UI 연결")]
    [SerializeField] private Text goldText;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        _currentGold = _startGold;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        _currentGold += amount;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (_currentGold < amount)
        {
            Debug.Log("골드가 부족합니다!");
            return false;
        }
        _currentGold -= amount;
        UpdateGoldUI();
        return true;
    }

    public int GetCurrentGold()
    {
        return _currentGold;
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"[ {_currentGold} G ]"; 
    }

    public void ResetGold()
    {
        _currentGold = _startGold;
        UpdateGoldUI();
    }
}