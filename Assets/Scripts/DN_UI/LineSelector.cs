using UnityEngine;
using UnityEngine.UI;

public class LineSelector : MonoBehaviour 
{
    [Header("제어 버튼 2개")]
    [SerializeField] private Button buttonUp;
    [SerializeField] private Button buttonDown;


    [Header("상태 표시 이미지 2개")]
    [SerializeField] private GameObject imageUp;
    [SerializeField] private GameObject imageDown;

    public bool IsUpperLine { get; private set; } = true;

    private void Start()
    {
        buttonUp.onClick.AddListener(() => ChangeLine(true));
        buttonDown.onClick.AddListener(() => ChangeLine(false));


        UpdateVisual();
    }

    private void ChangeLine(bool isUp)
    {
        if (IsUpperLine == isUp) return;

        IsUpperLine = isUp;
        UpdateVisual();

        Debug.Log($"출격 방향 변경 {(IsUpperLine ? " 위쪽 화살표만 활성화" : "아래쪽 화살표만 활성화")}");
    }

    private void UpdateVisual()
    {
        if (imageUp == null || imageDown == null) return;

        imageUp.SetActive(IsUpperLine);
        imageDown.SetActive(!IsUpperLine);

    }

}
