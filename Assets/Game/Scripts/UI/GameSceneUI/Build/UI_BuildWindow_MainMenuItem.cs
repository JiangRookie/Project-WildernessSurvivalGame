using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_MainMenuItem : MonoBehaviour
{
    [SerializeField] Image m_BgImage;
    [SerializeField] Button m_Button;
    [SerializeField] Image m_IconImage;
    [SerializeField] Sprite[] m_BgSprites;
    public BuildType BuildType { get; private set; }
    UI_BuildWindow m_OwnerWindow;

    public void Init(BuildType buildType, UI_BuildWindow ownerWindow)
    {
        BuildType = buildType;
        m_OwnerWindow = ownerWindow;
        this.BindMouseEffect();
        m_Button.onClick.AddListener(OnClick);
        OnUnSelect();
    }

    void OnClick()
    {
        m_OwnerWindow.SelectMainMenuItem(this);
    }

    public void OnSelect()
    {
        m_BgImage.sprite = m_BgSprites[1];
    }

    public void OnUnSelect()
    {
        m_BgImage.sprite = m_BgSprites[0];
    }
}