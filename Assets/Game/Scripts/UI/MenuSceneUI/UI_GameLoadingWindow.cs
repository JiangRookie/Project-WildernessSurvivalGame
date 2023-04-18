using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIElement(true, "UI/UI_GameLoadingWindow", 4)]
public class UI_GameLoadingWindow : UI_WindowBase
{
    [SerializeField] Text m_ProgressText;
    [SerializeField] Image m_FillImage;

    public override void OnShow()
    {
        base.OnShow();
        UpdateGameLoadingProgress(0);
    }

    public void UpdateGameLoadingProgress(float progressValue)
    {
        m_ProgressText.text = (int)progressValue + "%";
        m_FillImage.fillAmount = (int)progressValue;
    }
}