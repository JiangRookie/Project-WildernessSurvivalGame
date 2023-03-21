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
        UpdateProgress(0);
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateProgress(int progressValue)
    {
        m_ProgressText.text = progressValue + "%";
        m_FillImage.fillAmount = progressValue;
    }
}