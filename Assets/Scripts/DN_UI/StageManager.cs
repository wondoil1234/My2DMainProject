using System.Runtime.CompilerServices;
using UnityEngine;

public class StageManager : MonoBehaviour
{
   public static StageManager Inst {  get; private set; }

    [Header("라이프 설정")]
    [SerializeField] private int _maxBaseLife = 5;
    private int _currentBaseLife;

    [Header("[게임상태]")]
    private bool _isGameOver = false;

    private void Awake()
    {
        Inst = this;
        _currentBaseLife = _maxBaseLife;
    }

    public void DecreaseBaseLife(int amount = 1)
    {
        if (_isGameOver) return;

        _currentBaseLife -= amount;
        Debug.Log($"[기지 공격] 기지가 공격받습니다 남은 목숨 : {_currentBaseLife} / {_maxBaseLife}");

        if (_currentBaseLife <= 0)
        {
            _currentBaseLife = 0;
            ProcessGameOver();
        }
    }

    private void ProcessGameOver()
    {
        _isGameOver = true;

        Time.timeScale = 0f;

        Debug.LogError("기지가 파괴 되었습니다!");
    }
}
