using JKFrame;
using UnityEngine;

public enum CursorStyle { Normal = 0, Handle = 1 }

public class GameManager : SingletonMono<GameManager>
{
    #region 鼠标指针

    void Start()
    {
        Init();
    }

    void Init()
    {
        SetCursorStyle(CursorStyle.Normal);
    }

    [SerializeField] Texture2D[] m_CursorTextures;
    Texture2D m_CurrCursorTexture;
    CursorStyle m_CurrentCursorStyle;

    public void SetCursorStyle(CursorStyle cursorStyle)
    {
        if (cursorStyle == m_CurrentCursorStyle) return;
        m_CurrentCursorStyle = cursorStyle;
        m_CurrCursorTexture = m_CursorTextures[(int)cursorStyle];
        Cursor.SetCursor(m_CurrCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    #endregion

    #region 跨场景

    /// <summary>
    /// Create a new archive and start a new game
    /// </summary>
    public void StartGame(int mapSize, int mapSeed, int spawnSeed, float marshLimit)
    {
        ArchiveManager.Instance.CreateNewArchive(mapSize, mapSeed, spawnSeed, marshLimit);
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Use the last archive to enter the game
    /// </summary>
    public void ContinueGame()
    {
        ArchiveManager.Instance.LoadArchive();
        SceneManager.LoadScene("Game");
    }

    public void BackToMainMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    #endregion
}