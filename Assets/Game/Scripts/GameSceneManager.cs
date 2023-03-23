using JKFrame;
using Project_WildernessSurvivalGame;

/// <summary>
/// 游戏场景管理器
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    void Start()
    {
        #region Test

        if (IsTest)
        {
            if (IsCreateNewArchive)
            {
                ArchiveManager.Instance.CreateNewArchive(10, 1, 1, 0.75f);
            }
            else
            {
                ArchiveManager.Instance.LoadCurrentArchive();
            }
        }

        #endregion

        UIManager.Instance.CloseAll();
        StartGame();
    }

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    void StartGame()
    {
        IsInitialized = false;

        // 加载进度条
        m_LoadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();
        m_LoadingWindow.UpdateProgress(0);

        // 确定地图初始化配置数据
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.MAP);
        float mapSizeOnWorld
            = ArchiveManager.Instance.MapInitData.MapSize * mapConfig.MapChunkSize * mapConfig.CellSize;

        // 初始化角色、相机
        PlayerController.Instance.Init(mapSizeOnWorld);
        CameraController.Instance.Init(mapSizeOnWorld);

        // 初始化地图
        MapManager.Instance.UpdateViewer(PlayerController.Instance.transform);
        MapManager.Instance.Init();

        // 初始化快捷栏 UI
        UIManager.Instance.Show<UI_InventoryWindow>();

        // 显示主信息面板
        UIManager.Instance.Show<UI_MainInfoWindow>();

        // 初始化时间
        TimeManager.Instance.Init();
    }

    #region 测试逻辑

    public bool IsTest = true;
    public bool IsCreateNewArchive;

    #endregion

    #region 加载进度

    UI_GameLoadingWindow m_LoadingWindow;
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="current"></param>
    /// <param name="max"></param>
    public void UpdateMapProgress(int current, int max)
    {
        float temp = max;
        int currentProgress = (int)(100 / temp * current);
        if (current == max)
        {
            m_LoadingWindow.UpdateProgress(100);
            IsInitialized = true;
            m_LoadingWindow.Close();
            m_LoadingWindow = null;
        }
        else
        {
            m_LoadingWindow.UpdateProgress(currentProgress);
        }
    }

    #endregion
}