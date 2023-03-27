using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// 可序列化的字典
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    List<TKey> m_Keys;
    List<TValue> m_Values;

    [NonSerialized] Dictionary<TKey, TValue> m_Dictionary;

    public Dictionary<TKey, TValue> Dictionary => m_Dictionary;

    public SerializableDictionary() => m_Dictionary = new Dictionary<TKey, TValue>();

    public SerializableDictionary(Dictionary<TKey, TValue> dictionary) => m_Dictionary = dictionary;

    [OnSerializing]
    void OnSerializing(StreamingContext content)
    {
        m_Keys = new List<TKey>(m_Dictionary.Count);
        m_Values = new List<TValue>(m_Dictionary.Count);
        foreach (var (key, value) in m_Dictionary)
        {
            m_Keys.Add(key);
            m_Values.Add(value);
        }
    }

    [OnDeserialized]
    void OnDeserialized(StreamingContext content)
    {
        m_Dictionary = new Dictionary<TKey, TValue>(m_Keys.Count);
        for (int i = 0; i < m_Keys.Count; i++)
        {
            m_Dictionary.Add(m_Keys[i], m_Values[i]);
        }
    }
}