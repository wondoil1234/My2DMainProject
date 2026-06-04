using UnityEngine;

public class RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_GameQuit;

    private GameObject lifeContainer;
    private GameObject wavePanel;
    private GameObject goldPanel;

    private void Start()
    {
        lifeContainer = GameObject.Find("LifeContainer");
        wavePanel = GameObject.Find("WavePanel");
        goldPanel = GameObject.Find("GoldPanel");

        if (lifeContainer != null) lifeContainer.SetActive(false);
        if (wavePanel != null) wavePanel.SetActive(false);
        if (goldPanel != null) goldPanel.SetActive(false);
    }

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_GameQuit.BindOnClickButtonEvent(OnClick_GameQuit);
    }

    public void OnClick_GameStart()
    {
        Debug.Log("게임을 시작합니다.");

        if (lifeContainer != null) lifeContainer.SetActive(true);
        if (wavePanel != null) wavePanel.SetActive(true);
        if (goldPanel != null) goldPanel.SetActive(true);

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