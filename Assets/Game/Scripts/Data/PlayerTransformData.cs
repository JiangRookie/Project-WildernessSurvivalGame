using System;
using UnityEngine;

/// <summary>
/// 玩家位置数据
/// </summary>
[Serializable]
public class PlayerTransformData
{
    SerializableVector3 m_Position;

    public Vector3 Position
    {
        get => m_Position.Convert2Vector3();
        set => m_Position = value.Convert2SerializableVector3();
    }

    SerializableVector3 m_Rotation;

    public Vector3 Rotation
    {
        get => m_Rotation.Convert2Vector3();
        set => m_Rotation = value.Convert2SerializableVector3();
    }
}

[Serializable]
public class PlayerCoreData
{
    public float Hp;
    public float Hungry;
}