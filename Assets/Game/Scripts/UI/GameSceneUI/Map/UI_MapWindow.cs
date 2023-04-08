using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

namespace Project_WildernessSurvivalGame
{
    /// <summary>
    /// UI地图窗口
    /// </summary>
    [UIElement(true, "UI/UI_MapWindow", 4)]
    public class UI_MapWindow : UI_WindowBase
    {
        #region Field

        [SerializeField] GameObject m_MapItemPrefab; // 单个地图块在UI中的预制体
        [SerializeField] GameObject m_MapIconPrefab; // 单个Icon在UI中的预制体
        [SerializeField] RectTransform m_Content;    // 所有地图块、Icon显示的父物体
        [SerializeField] RectTransform m_PlayerIcon; // 玩家所在位置的Icon

        int m_MapChunkSize;
        float m_ContentSize;
        float m_MapChunkImageSize;
        float m_MapSizeOnWorld;
        Sprite m_ForestSprite;
        Dictionary<ulong, Image> m_MapObjectIconDict = new Dictionary<ulong, Image>(); // 所有的地图物体的Icon字典
        float m_ScrollValue;
        float m_MinScaleFactor;

        const string MouseScrollWheel = "Mouse ScrollWheel";
        const string ScrollView = "Scroll View";
        const float ScrollViewSize = 1050f;
        const float MaxScaleFactor = 10f;
        const int ContentScaleFactor = 10;

        #endregion

        void Update()
        {
            m_ScrollValue = Input.GetAxis(MouseScrollWheel); // 这里会一直监听鼠标滑轮事件
            if (m_ScrollValue != 0)
            {
                float newScale = Mathf.Clamp(m_Content.localScale.x + m_ScrollValue, m_MinScaleFactor, MaxScaleFactor);
                m_Content.localScale = new Vector3(newScale, newScale, 0);
            }
        }

        public override void Init()
        {
            transform.Find(ScrollView).GetComponent<ScrollRect>().onValueChanged.AddListener(UpdatePlayerIconPos);
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        /// <param name="mapSize">地图中一行地图块的数量</param>
        /// <param name="mapChunkSize">地图块中一行格子的数量</param>
        /// <param name="mapSizeOnWorld">地图的实际尺寸，地图块尺寸 * 地图块数量</param>
        /// <param name="forestTexture">森林贴图</param>
        public void InitMap(float mapSize, int mapChunkSize, float mapSizeOnWorld, Texture2D forestTexture)
        {
            m_MapChunkSize = mapChunkSize;
            m_MapSizeOnWorld = mapSizeOnWorld;
            m_ForestSprite = CreateSprite(forestTexture);

            m_ContentSize = mapSizeOnWorld * ContentScaleFactor;                   // 将 Content 容器的大小设置为地图大小的 10 倍
            m_Content.sizeDelta = new Vector2(m_ContentSize, m_ContentSize);       // 这个 RectTransform 大小相对于锚点之间的距离。
            m_Content.localScale = new Vector3(MaxScaleFactor, MaxScaleFactor, 1); // 打开默认最大缩放
            m_MapChunkImageSize = m_ContentSize / mapSize;                         // 计算单个地图块 UI 的尺寸
            m_MinScaleFactor = ScrollViewSize / m_ContentSize;
        }

        /// <summary>
        /// 更新 Content 的中心点，为了鼠标缩放的时候，中心点是玩家所处的位置
        /// </summary>
        /// <param name="viewerPos">观察者位置</param>
        public void UpdatePivot(Vector3 viewerPos)
        {
            float x = viewerPos.x / m_MapSizeOnWorld;
            float z = viewerPos.z / m_MapSizeOnWorld;
            m_Content.pivot = new Vector2(x, z); // 修改 Content.pivot 会触发 Scroll Rect 组件的 OnValueChanged 事件
        }

        /// <summary>
        /// 添加一个地图块
        /// </summary>
        /// <param name="chunkIndex">地图块索引</param>
        /// <param name="mapObjectDict">地图块中的各种地图对象组合成的列表</param>
        /// <param name="texture">纹理</param>
        public void AddMapChunk(Vector2Int chunkIndex, SerializableDictionary<ulong, MapObjectData> mapObjectDict, Texture2D texture = null)
        {
            // 获取地图块 UI 的 RectTransform 并设置地图块 UI 的位置和大小
            RectTransform mapChunkRect = Instantiate(m_MapItemPrefab, m_Content).GetComponent<RectTransform>();
            mapChunkRect.anchoredPosition = new Vector2(chunkIndex.x * m_MapChunkImageSize, chunkIndex.y * m_MapChunkImageSize);
            mapChunkRect.sizeDelta = new Vector2(m_MapChunkImageSize, m_MapChunkImageSize);

            var mapChunkImage = mapChunkRect.GetComponent<Image>();
            if (texture == null) // 森林的情况
            {
                mapChunkImage.type = Image.Type.Tiled; // 将图片类型设置为平铺类型

                // 设置贴瓷砖的比例，要在一个Image中显示这个地图块所包含的格子数量
                var ratio = m_ForestSprite.texture.width / m_MapChunkImageSize;
                mapChunkImage.pixelsPerUnitMultiplier = m_MapChunkSize * ratio;
                mapChunkImage.sprite = m_ForestSprite;
            }
            else
            {
                mapChunkImage.sprite = CreateSprite(texture);
            }

            foreach (var mapObjectData in mapObjectDict.Dictionary.Values)
            {
                AddMapObjectIcon(mapObjectData);
            }
        }

        public void RemoveMapObjectIcon(ulong mapObjectID)
        {
            if (m_MapObjectIconDict.TryGetValue(mapObjectID, out Image icon))
            {
                icon.JKGameObjectPushPool();
                m_MapObjectIconDict.Remove(mapObjectID);
            }
        }

        public void AddMapObjectIcon(MapObjectData mapObjectData)
        {
            MapObjectConfig config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectData.ConfigID);
            if (config.MapIconSprite == null || config.IconSize <= 0) return;
            GameObject gameObj = PoolManager.Instance.GetGameObject(m_MapIconPrefab, m_Content);
            Image iconImage = gameObj.GetComponent<Image>();
            iconImage.sprite = config.MapIconSprite;
            iconImage.transform.localScale = Vector3.one * config.IconSize;

            // 因为 Content 的尺寸在初始化的时候 * ContentScaleFactor，所以 Icon 也需要乘上同样的系数
            var x = mapObjectData.Position.x * ContentScaleFactor;
            var y = mapObjectData.Position.z * ContentScaleFactor;
            gameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            m_MapObjectIconDict.Add(mapObjectData.ID, iconImage);
        }

        /// <summary>
        /// 将传入的 <paramref name="texture"/> 转换为 Sprite 图片
        /// </summary>
        /// <param name="texture">贴图</param>
        /// <returns>返回 Sprite 图片</returns>
        static Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create
            (
                texture: texture
              , rect: new Rect(0, 0, texture.width, texture.height)
              , pivot: new Vector2(0.5f, 0.5f)
            );
        }

        void UpdatePlayerIconPos(Vector2 value)
        {
            // 玩家的 Icon 完全放在 Content 的中心点
            // anchoredPosition：这个RectTransform的中心点相对于锚点参考点的位置。
            m_PlayerIcon.anchoredPosition3D = m_Content.anchoredPosition;
        }
    }
}