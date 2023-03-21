using JKFrame;
using Project_WildernessSurvivalGame;

/// <summary>
/// 游戏场景管理器
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    public bool IsInitialized { get; private set; }

    void Start()
    {
        UIManager.Instance.CloseAll();
        StartGame();
    }

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    void StartGame()
    {
        IsInitialized = false;

        // 加载进度条
        // 确定地图初始化配置数据
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.MAP);
        float mapSizeOnWorld
            = ArchiveManager.Instance.MapInitData.MapSize * mapConfig.MapChunkSize * mapConfig.CellSize;

        // 初始化角色、相机
        PlayerController.Instance.Init(mapSizeOnWorld);
        CameraController.Instance.Init(mapSizeOnWorld);

        MapManager.Instance.Init();
        MapManager.Instance.UpdateViewer(PlayerController.Instance.transform);
    }
}