using System;
using UnityEngine;

[Serializable]
public struct SerializableVector3
{
    public float X, Y, Z;

    public SerializableVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
}

[Serializable]
public struct SerializableVector2
{
    public float X, Y;

    public SerializableVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"({X}, {Y})";
}

public static class SerializableExtension
{
    public static Vector3 Convert2Vector3(this SerializableVector3 serializableVector3)
    {
        return new Vector3(serializableVector3.X, serializableVector3.Y, serializableVector3.Z);
    }

    public static SerializableVector3 Convert2SerializableVector3(this Vector3 vector3)
    {
        return new SerializableVector3(vector3.x, vector3.y, vector3.z);
    }

    public static Vector2 Convert2Vector2(this SerializableVector2 serializableVector2)
    {
        return new Vector2(serializableVector2.X, serializableVector2.Y);
    }

    public static SerializableVector2 Convert2SerializableVector2(this Vector2 vector2)
    {
        return new SerializableVector2(vector2.x, vector2.y);
    }
    
    public static Vector2Int Convert2Vector2Int(this SerializableVector2 serializableVector2)
    {
        return new Vector2Int((int)serializableVector2.X, (int)serializableVector2.Y);
    }

    public static SerializableVector2 Convert2SerializableVector2(this Vector2Int vector2)
    {
        return new SerializableVector2(vector2.x, vector2.y);
    }
}