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


        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.RobbyUI);
    }

    public void OnClick_GameQuit()
    {
        Debug.Log("게임을 종료합니다.");



        DaniTechGameManager.Inst.SaveAndEndGame();
    }



}
