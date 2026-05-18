using UnityEngine;

public class RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_GaemQuit;

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_GaemQuit.BindOnClickButtonEvent(OnClick_GaemQuit);
    }

    public void OnClick_GameStart()
    {

    }

    public void OnClick_GaemQuit()
    {

    }



}
