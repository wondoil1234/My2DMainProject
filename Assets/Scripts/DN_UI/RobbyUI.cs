using UnityEngine;
public class RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_GameQuit;

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_GameQuit.BindOnClickButtonEvent(OnClick_GameQuit);
    }

    public void OnClick_GameStart()
    {
        Debug.Log("게임을 시작합니다.");

        var gm = DaniTechGameManager.Inst;
        if (gm.lifeContainer != null) gm.lifeContainer.SetActive(true);
        if (gm.wavePanel != null) gm.wavePanel.SetActive(true);
        if (gm.goldPanel != null) gm.goldPanel.SetActive(true);

        var lifeContainerComp = gm.lifeContainer?.GetComponent<LifeContainer>();
        if (lifeContainerComp != null)
            lifeContainerComp.InitHearts(gm.maxLife);

        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.RobbyUI);
        if (WaveManager.Inst != null)
        {
            WaveManager.Inst.ResetWave();
            WaveManager.Inst.OnGameStart();
        }
        else
        {
            Debug.LogError("씬에 WaveManager 오브젝트가 없거나 생성되지 않았습니다!");
        }
    }

    public void OnClick_GameQuit()
    {
        Debug.Log("게임을 종료합니다.");
        DaniTechGameManager.Inst.SaveAndEndGame();
    }
}