using JKFrame;
using UnityEngine;

public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    UI_GameLoadingWindow m_LoadingWindow;
    int m_CurrMapChunkCount = 0;
    int m_MaxMapChunkCount = 0;
    bool m_IsGameOver = false;
    bool m_IsPause = false;
    public bool IsGameOver => m_IsGameOver;
    public bool IsInitialized { get; private set; }

    // #region 测试逻辑
    //
    // public bool IsTest = true;
    // public bool IsCreateNewArchive;
    //
    // #endregion

    void Start()
    {
        // #region Test
        //
        // if (IsTest)
        // {
        //     if (IsCreateNewArchive)
        //     {
        //         ArchiveManager.Instance.CreateNewArchive(10, 1, 1, 0.6f);
        //     }
        //     else
        //     {
        //         ArchiveManager.Instance.LoadCurrentArchive();
        //     }
        // }
        //
        // #endregion

        UIManager.Instance.CloseAll();
        StartGame();
    }

    void Update()
    {
        if (IsInitialized == false) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_IsPause = !m_IsPause;
            if (m_IsPause)
                PauseGame();
            else
                UnPauseGame();
        }
    }

    void StartGame()
    {
        IsInitialized = false;

        // Displays and loads the progress bar
        m_LoadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();

        // m_LoadingWindow.UpdateProgress(0);

        // Get map config data
        var mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        var mapSizeOnWorld = ArchiveManager.Instance.MapInitData.MapSize * mapConfig.MapChunkSize * mapConfig.CellSize;

        // 显示主信息面板：
        // 依赖于 TimeManager 的信息发送
        // 依赖于 PlayerController 的信息发送
        UIManager.Instance.Show<UI_MainInfoWindow>(); // Add、RemoveEvent

        // Initialize player、camera
        PlayerController.Instance.Init(mapSizeOnWorld); // EventTrigger
        CameraController.Instance.Init(mapSizeOnWorld);

        // Initialize time
        TimeManager.Instance.Init(); // EventTrigger

        // Initialize map
        MapManager.Instance.UpdateViewer(PlayerController.Instance.transform);
        MapManager.Instance.Init();

        // Initialize inventory
        InventoryManager.Instance.Init();

        // Initialize inputManager
        InputManager.Instance.Init();

        // Initialize buildManager
        BuildManager.Instance.Init();

        // Initialize scienceManager
        ScienceManager.Instance.Init();
    }

    void PauseGame()
    {
        m_IsPause = true;
        UIManager.Instance.Show<UI_PauseWindow>();
        Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        m_IsPause = false;
        UIManager.Instance.Close<UI_PauseWindow>();
        Time.timeScale = 1;
    }

    #region Archive

    public void GameOver()
    {
        m_IsGameOver = true;
        ArchiveManager.Instance.ClearArchive();
        Invoke(nameof(OnBackToMainMenuScene), 2f);
    }

    public void BackToMainMenuScene()
    {
        EventManager.EventTrigger(EventName.SaveGame);
        OnBackToMainMenuScene();
    }

    void OnApplicationQuit()
    {
        if (IsGameOver)
        {
            // 紧急存档
            EventManager.EventTrigger(EventName.SaveGame);
        }
    }

    void OnBackToMainMenuScene()
    {
        Time.timeScale = 1;

        // 回收场景资源
        MapManager.Instance.OnCloseGameScene();
        EventManager.Clear();
        MonoManager.Instance.StopAllCoroutines();
        UIManager.Instance.CloseAll();

        GameManager.Instance.BackToMainMenuScene();
    }

    #endregion

    #region Progress

    public void UpdateGameLoadingProgress()
    {
        m_CurrMapChunkCount++;
        UpdateGameLoadingProgress(m_CurrMapChunkCount, m_MaxMapChunkCount);
    }

    /// <summary>
    /// Set the maximum progress bar value to the number of map chunks
    /// </summary>
    /// <param name="max">map chunk number</param>
    public void SetProgressBarMaxValue(int max)
    {
        m_MaxMapChunkCount = max;
    }

    void UpdateGameLoadingProgress(int current, int max)
    {
        float currProgress = 100 / max * current;
        if (current == max)
        {
            m_LoadingWindow.UpdateGameLoadingProgress(100);
            IsInitialized = true;
            m_LoadingWindow.Close();
            m_LoadingWindow = null;
        }
        else
        {
            m_LoadingWindow.UpdateGameLoadingProgress(currProgress);
        }
    }

    #endregion

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }
}