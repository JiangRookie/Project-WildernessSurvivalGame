using JKFrame;
using UnityEngine;

public enum BuildType { Weapon, Building, Crop }

[UIElement(false, "UI/UI_BuildWindow", 1)]
public class UI_BuildWindow : UI_WindowBase
{
    // 所有的一级菜单选项
    [SerializeField] UI_BuildWindow_MainMenuItem[] m_MainMenuItems;
    [SerializeField] UI_BuildWindow_SecondaryMenu m_SecondaryMenu;
    UI_BuildWindow_MainMenuItem m_CurrentSelectedMainMenuItem;

    public override void Init()
    {
        // 初始化一级菜单全部选项
        for (int i = 0; i < m_MainMenuItems.Length; i++)
        {
            m_MainMenuItems[i].Init((BuildType)i, this);
        }
        m_SecondaryMenu.Init();
    }

    /// <summary>
    /// 选择菜单选项
    /// </summary>
    /// <param name="newMenuItem"></param>
    public void SelectMainMenuItem(UI_BuildWindow_MainMenuItem newMenuItem)
    {
        if (m_CurrentSelectedMainMenuItem != null) m_CurrentSelectedMainMenuItem.OnUnSelect();
        m_CurrentSelectedMainMenuItem = newMenuItem;
        m_CurrentSelectedMainMenuItem.OnSelect();

        Debug.Log("开启二级菜单：" + m_CurrentSelectedMainMenuItem.BuildType.ToString());
        m_SecondaryMenu.Show(newMenuItem.BuildType);
    }
}