using JKFrame;
using UnityEngine;

public enum CursorState
{
    Normal = 0, Handle = 1
}

public class GameManager : SingletonMono<GameManager>
{
    #region 鼠标指针

    [SerializeField] Texture2D[] m_CursorTextures;

    void Start()
    {
        SetCursorState(CursorState.Normal);
    }

    public void SetCursorState(CursorState state)
    {
        Texture2D texture2D = m_CursorTextures[(int)state];
        Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto);
    }

    #endregion
}