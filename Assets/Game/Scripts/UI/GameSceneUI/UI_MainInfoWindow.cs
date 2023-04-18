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

    PlayerConfig m_PlayerConfig;

    public override void OnInit()
    {
        m_PlayerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
    }

    protected override void RegisterEventListener()
    {
        base.RegisterEventListener();
        EventManager.AddEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.AddEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
        EventManager.AddEventListener<float>(EventName.UpdatePlayerHp, UpdatePlayerHp);
        EventManager.AddEventListener<float>(EventName.UpdatePlayerHungry, UpdatePlayerHungry);
    }

    protected override void CancelEventListener()
    {
        base.CancelEventListener();
        EventManager.RemoveEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.RemoveEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
        EventManager.RemoveEventListener<float>(EventName.UpdatePlayerHp, UpdatePlayerHp);
        EventManager.RemoveEventListener<float>(EventName.UpdatePlayerHungry, UpdatePlayerHungry);
    }

    void UpdateTimeState(bool isSun)
    {
        m_TimeStateImage.sprite = m_TimeStateSprites[isSun ? 0 : 1];
    }

    void UpdateDayNum(int dayNum)
    {
        m_DayNumText.text = "Day " + (dayNum + 1).ToString();
    }

    void UpdatePlayerHp(float hp)
    {
        m_HpFillImage.fillAmount = hp / m_PlayerConfig.MaxHp;
    }

    void UpdatePlayerHungry(float hungry)
    {
        m_HungryFillImage.fillAmount = hungry / m_PlayerConfig.MaxHungry;
    }
}