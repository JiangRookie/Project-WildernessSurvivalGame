using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    /// <summary>
    /// 地图物品配置
    /// </summary>
    [CreateAssetMenu(fileName = "地图物品配置", menuName = "Config/地图物品配置")]
    public class MapObjectConfig : ConfigBase // Rename: 考虑 MapObjectConfig -> MapItemConfig
    {
        [LabelText("空的 不生成物品")] public bool IsEmpty = false;
        [LabelText("所在的地图顶点类型")] public MapVertexType MapVertexType;
        [LabelText("生成的预制体")] public GameObject Prefab;
        [LabelText("在UI地图上的Icon")] public Sprite MapIconSprite;
        [LabelText("UI地图Icon尺寸")] public float IconSize = 1;
        [LabelText("生成概率 权重类型")] public int Probability;
    }
}