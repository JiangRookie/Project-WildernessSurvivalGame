using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIElement(isCache: false, resPath: "UI/UI_MenuSceneMainWindow", layerNum: 1)]
public class UI_MenuSceneMainWindow : UI_WindowBase
{
    [SerializeField] Button m_NewGameButton;
    [SerializeField] Button m_ContinueGameButton;
    [SerializeField] Button m_QuitGameButton;

    public override void Init()
    {
        m_NewGameButton.onClick.AddListener(NewGame);
        m_ContinueGameButton.onClick.AddListener(ContinueGame);
        m_QuitGameButton.onClick.AddListener(QuitGame);

        m_NewGameButton.BindMouseEffect();
        m_ContinueGameButton.BindMouseEffect();
        m_QuitGameButton.BindMouseEffect();
    }

    public override void OnClose()
    {
        base.OnClose();
        m_NewGameButton.RemoveMouseEffect();
        m_ContinueGameButton.RemoveMouseEffect();
        m_QuitGameButton.RemoveMouseEffect();
    }

    public override void OnShow()
    {
        base.OnShow();

        // 当前是否需要显示”继续游戏“按钮 
        m_ContinueGameButton.gameObject.SetActive(ArchiveManager.Instance.HasArchived);
    }

    void NewGame()
    {
        UIManager.Instance.Show<UI_NewGameWindow>();
        Close();
    }

    void ContinueGame()
    {
        Close();
        GameManager.Instance.UseCurrentArchiveEnterGame();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}