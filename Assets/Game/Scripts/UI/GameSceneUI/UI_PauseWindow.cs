using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIElement(false, "UI/UI_PauseWindow", 4)]
public class UI_PauseWindow : UI_WindowBase
{
    [SerializeField] Button m_ContinueButton;
    [SerializeField] Button m_QuitButton;

    public override void OnInit()
    {
        m_ContinueButton.onClick.AddListener(ContinueButtonClick);
        m_QuitButton.onClick.AddListener(QuitButtonClick);
    }

    void ContinueButtonClick()
    {
        GameSceneManager.Instance.UnPauseGame();
    }

    void QuitButtonClick()
    {
        GameSceneManager.Instance.BackToMainMenuScene();
    }
}