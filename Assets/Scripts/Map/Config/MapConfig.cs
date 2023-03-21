using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    /// <summary>
    /// 地图配置
    /// </summary>
    [CreateAssetMenu(fileName = "地图配置", menuName = "Config/地图配置")]
    public class MapConfig : ConfigBase
    {
        [LabelText("一个地图块的格子数量")] public int MapChunkSize;
        [LabelText("格子大小")] public float CellSize;
        [LabelText("噪音间隔")] public float NoiseLacunarity;

        [LabelText("森林贴图")] public Texture2D ForestTexture;
        [LabelText("沼泽贴图")] public Texture2D[] MarshTextures;
        [LabelText("地图材质")] public Material MapMaterial;

        [LabelText("玩家可视距离")] public int ViewDistance;
    }
}