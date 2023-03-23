using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIElement(false, "UI/UI_MainInfoWindow", 0)]
public class UI_MainInfoWindow : UI_WindowBase
{
    [SerializeField] Image m_TimeStateImage;
    [SerializeField] Sprite[] m_TimeStateSprites;
    [SerializeField] Text m_DayNumText;
    [SerializeField] Image m_HungryFillImage;
    [SerializeField] Image m_HpFillImage;

    protected override void RegisterEventListener()
    {
        base.RegisterEventListener();
        EventManager.AddEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.AddEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
    }

    protected override void CancelEventListener()
    {
        base.CancelEventListener();
        EventManager.AddEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.AddEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
    }

    void UpdateTimeState(bool isSun)
    {
        m_TimeStateImage.sprite = m_TimeStateSprites[isSun ? 0 : 1];
    }

    void UpdateDayNum(int dayNum)
    {
        m_DayNumText.text = "Day " + (dayNum + 1).ToString();
    }
}