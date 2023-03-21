using JKFrame;
using UnityEngine;
using UnityEngine.UI;

[UIElement(isCache: false, resPath: "UI/UI_NewGameWindow", layerNum: 1)]
public class UI_NewGameWindow : UI_WindowBase
{
    [SerializeField] Slider m_MapSizeSlider;
    [SerializeField] Slider m_MarshLimitSlider;
    [SerializeField] InputField m_MapSeedInputField;
    [SerializeField] InputField m_SpawnSeedInputField;
    [SerializeField] Button m_BackButton;
    [SerializeField] Button m_StartGameButton;

    public override void Init()
    {
        m_BackButton.onClick.AddListener(Close);
        m_StartGameButton.onClick.AddListener(StartGame);

        m_BackButton.BindMouseEffect();
        m_StartGameButton.BindMouseEffect();
    }

    public override void OnClose()
    {
        base.OnClose();
        m_BackButton.RemoveMouseEffect();
        m_StartGameButton.RemoveMouseEffect();
        UIManager.Instance.Show<UI_MenuSceneMainWindow>();
    }

    void StartGame()
    {
        int mapSize = (int)m_MapSizeSlider.value;
        float marshLimit = m_MarshLimitSlider.value;
        int mapSeed = string.IsNullOrEmpty(m_MapSeedInputField.text)
            ? Random.Range(int.MinValue, int.MaxValue)
            : int.Parse(m_MapSeedInputField.text);
        int spawnSeed = string.IsNullOrEmpty(m_SpawnSeedInputField.text)
            ? Random.Range(int.MinValue, int.MaxValue)
            : int.Parse(m_SpawnSeedInputField.text);
        UIManager.Instance.CloseAll();
        GameManager.Instance.CreateNewArchiveEnterGame(mapSize, mapSeed, spawnSeed, marshLimit);
    }
}