using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_SecondaryMenuItem : MonoBehaviour
{
    [SerializeField] Image m_BgImage;
    [SerializeField] Image m_IconImage;
    [SerializeField] Button m_Button;
    [SerializeField] Sprite[] m_BgSprites;
    public BuildConfig BuildConfig { get; private set; } // 当前选项代表的建造配置
    UI_BuildWindow_SecondaryMenu m_OwnerWindow;
    bool m_IsMeetCondition;

    void Start()
    {
        this.BindMouseEffect();
        m_Button.onClick.AddListener(OnClick);
    }

    public void Init(BuildConfig buildConfig, UI_BuildWindow_SecondaryMenu ownerWindow, bool isMeetCondition)
    {
        BuildConfig = buildConfig;
        m_OwnerWindow = ownerWindow;
        m_IsMeetCondition = isMeetCondition;
        m_IconImage.sprite = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.ITEM, buildConfig.TargetID).Icon;
        m_IconImage.color = isMeetCondition ? Color.white : Color.black;
        UnSelect();
    }

    void OnClick()
    {
        m_OwnerWindow.SelectSecondaryMenuItem(this);
    }

    public void Select()
    {
        m_BgImage.sprite = m_BgSprites[1];
    }

    public void UnSelect()
    {
        m_BgImage.sprite = m_BgSprites[0];
    }
}