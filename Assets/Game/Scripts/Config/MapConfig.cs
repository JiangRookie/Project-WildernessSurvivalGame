using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

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
    [LabelText("地图早晨刷新概率 1/x")] public int RefreshProbability;

    [Header("AI")]
    [LabelText("地图块AI数量限制")]
    public int MaxAiCountOnChunk;

    [LabelText("地图块森林/沼泽生成AI的最小顶点数")] public int GenerateAiMinVertexCountOnChunk;
}