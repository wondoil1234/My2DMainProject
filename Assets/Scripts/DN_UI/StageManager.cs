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

        var lifeContainer = FindObjectOfType<LifeContainer>();
        if (lifeContainer != null)
            lifeContainer.InitHearts(_maxBaseLife);

        ClearRemainingHPBars();
    }

    public void DecreaseBaseLife(int amount = 1)
    {
        if (_isGameOver) return;
        _currentBaseLife -= amount;

        // 하트 업데이트
        var lifeContainer = FindObjectOfType<LifeContainer>();
        if (lifeContainer != null)
            lifeContainer.UpdateHearts(_currentBaseLife);

        Debug.Log($"[기지 공격] 남은 목숨 : {_currentBaseLife} / {_maxBaseLife}");
        DaniTechGameManager.Inst.LoseLife();
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

        DaniTechGameManager.Inst.TriggerGameOver();
    }

    public void ResetGame()
    {
        _isGameOver = false;
        _currentBaseLife = _maxBaseLife;

        ClearRemainingHPBars();
    }

    public void ClearRemainingHPBars()
    {
        HudUI hudUI =  FindObjectOfType<HudUI>();

        if (hudUI != null)
        {
            hudUI.ClearAllSlots();
        }
        else
        {
            Debug.LogWarning("[StageManager] 씬에서 HudUI 관리자를 찾을 수 없습니다.");
        }
    }

}
