using JKFrame;
using UnityEngine;

public enum CursorState
{
    Normal = 0, Handle = 1
}

public class GameManager : SingletonMono<GameManager>
{
    [SerializeField] Texture2D[] m_CursorTextures;

    void Start()
    {
        Init();
    }

    void Init()
    {
        SetCursorState(CursorState.Normal);
    }

    #region 鼠标指针

    public void SetCursorState(CursorState state)
    {
        Texture2D texture2D = m_CursorTextures[(int)state];
        Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto);
    }

    #endregion

    #region 跨场景

    public void CreateNewArchiveEnterGame(int mapSize, int mapSeed, int spawnSeed, float marshLimit)
    {
        ArchiveManager.Instance.CreateNewArchive(mapSize, mapSeed, spawnSeed, marshLimit);
        SceneManager.LoadScene("Game");
    }

    public void UseCurrentArchiveEnterGame()
    {
        ArchiveManager.Instance.LoadCurrentArchive();
        SceneManager.LoadScene("Game");
    }

    #endregion
}