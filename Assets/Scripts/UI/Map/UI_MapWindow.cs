using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI地图窗口
/// </summary>
[UIElement(true, "UI/UI_MapWindow", 4)]
public class UI_MapWindow : UI_WindowBase
{
    [SerializeField] RectTransform Content; // 所有地图块、Icon显示的父物体
    float m_ContentSize;
    [SerializeField] GameObject MapItemPrefab;            // 单个地图块在UI中的预制体
    [SerializeField] GameObject MapIconPrefab;            // 单个Icon在UI中的预制体
    [SerializeField] RectTransform PlayerIcon;            // 玩家所在位置的Icon
    Dictionary<Vector2Int, Image> m_MapImageDict = new(); // 地图图片字典
    float m_MapChunkImageSize;                            // UI地图块图片的尺寸
    int m_MapChunkSize;                                   // 一个地图块的格子数量
    float m_MapSizeOnWorld;                               // 地图在世界中的尺寸
    Sprite m_ForestSprite;                                // 森林地块的精灵
    float m_MinScale;                                     // 最小的放大倍数
    float m_MaxScale;                                     // 最大的放大倍数

    public override void Init()
    {
        transform.Find("Scroll View").GetComponent<ScrollRect>().onValueChanged.AddListener(UpdatePlayerIconPos);
    }

    /// <summary>
    /// 初始化地图
    /// </summary>
    /// <param name="mapSize">地图一行或一列有多少个Image/Chunk</param>
    /// <param name="mapChunkSize"></param>
    /// <param name="mapSizeOnWorld">地图在世界中一行或一列有多大</param>
    /// <param name="forestTexture">森林的贴图</param>
    public void InitMap(float mapSize, int mapChunkSize, float mapSizeOnWorld, Texture2D forestTexture)
    {
        m_MapSizeOnWorld = mapSizeOnWorld;
        m_ForestSprite = CreateMapSprite(forestTexture);

        m_MapChunkSize = mapChunkSize;

        // 计算内容尺寸
        m_ContentSize = mapSizeOnWorld * 10;
        Content.sizeDelta = new Vector2(m_ContentSize, m_ContentSize);

        // 计算一个UI地图块的尺寸
        m_MapChunkImageSize = m_ContentSize / mapSize;
        m_MinScale = 1050f / m_ContentSize; // 1050f 是 Scroll View 的大小
    }

    /// <summary>
    /// 更新中心点，为了鼠标缩放的时候，中心点是玩家所处的位置
    /// </summary>
    /// <param name="viewerPos">观察者位置</param>
    public void UpdatePivot(Vector3 viewerPos)
    {
        var x = viewerPos.x / m_MapSizeOnWorld;
        var z = viewerPos.z / m_MapSizeOnWorld;
        // 修改 Content 后会触发 Scroll Rect 组件的 OnValueChanged 事件
        Content.pivot = new Vector2(x, z);
    }

    /// <summary>
    /// 添加一个地图块
    /// </summary>
    public void AddMapChunk(Vector2Int chunkIndex, List<MapChunkMapObjectModel> mapObjectList, Texture2D texture = null)
    {
        var mapChunkRect = Instantiate(MapItemPrefab, Content).GetComponent<RectTransform>();

        // 确定地图块的Image的坐标和宽高
        mapChunkRect.anchoredPosition =
            new Vector2(chunkIndex.x * m_MapChunkImageSize, chunkIndex.y * m_MapChunkImageSize);
        mapChunkRect.sizeDelta = new Vector2(m_MapChunkImageSize, m_MapChunkImageSize);

        var mapChunkImage = mapChunkRect.GetComponent<Image>();

        // 森林的情况
        if (texture == null)
        {
            mapChunkImage.type = Image.Type.Tiled;

            // 设置贴瓷砖的比例，要在一个Image中显示这个地图块所包含的格子数量
            var ratio = m_ForestSprite.texture.width / m_MapChunkImageSize; // 贴图与Image的比例

            // 一个地图块上有多少个格子
            mapChunkImage.pixelsPerUnitMultiplier = m_MapChunkSize * ratio;
            mapChunkImage.sprite = m_ForestSprite;
        }
        else
        {
            mapChunkImage.sprite = CreateMapSprite(texture);
        }

        // TODO：添加物体的Icon

        // TODO：待重构，因为肯定还需要保存Icon的信息用来后续移除（因为Icon代表的花草树木有可能会消失）
        m_MapImageDict.Add(chunkIndex, mapChunkImage);
    }

    /// <summary>
    /// 生成地图精灵
    /// </summary>
    /// <param name="texture">贴图</param>
    /// <returns>将传入的<paramref name="texture"/>贴图转换为Sprite图片并返回</returns>
    static Sprite CreateMapSprite(Texture2D texture)
    {
        return Sprite.Create
        (
            texture: texture
          , rect: new Rect(0, 0, texture.width, texture.height)
          , pivot: new Vector2(0.5f, 0.5f)
        );
    }

    public void UpdatePlayerIconPos(Vector2 value)
    {
        // 玩家的 Icon 完全放在 Content 的中心点
        PlayerIcon.anchoredPosition3D = Content.anchoredPosition;
    }
}