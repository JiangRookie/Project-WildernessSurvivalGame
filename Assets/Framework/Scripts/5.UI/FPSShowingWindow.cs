using UnityEngine;

namespace JKFrame
{
    public class FPSShowingWindow : MonoBehaviour
    {
        /// <summary>
        /// 上一次更新帧率的时间
        /// </summary>
        float m_LastUpdateShowTime = 0f;

        /// <summary>
        /// 更新显示帧率的时间间隔
        /// </summary>
        const float UpdateTime = 0.05f;

        /// <summary>
        /// 帧数
        /// </summary>
        int m_Frames = 0;

        /// <summary>
        /// 帧间间隔
        /// </summary>
        float m_frameDeltaTime = 0;

        float m_FPS = 0;
        Rect m_fps, m_dtime;
        GUIStyle m_Style = new GUIStyle();

        void Start()
        {
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
            m_fps = new Rect(0, 0, 100, 100);
            m_dtime = new Rect(0, 100, 100, 100);
            m_Style.fontSize = 100;
            m_Style.normal.textColor = Color.red;
        }

        void Update()
        {
            m_Frames++;
            if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= UpdateTime)
            {
                m_FPS = m_Frames / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
                m_frameDeltaTime = (Time.realtimeSinceStartup - m_LastUpdateShowTime) / m_Frames;
                m_Frames = 0;
                m_LastUpdateShowTime = Time.realtimeSinceStartup;
            }
        }

        void OnGUI()
        {
            GUI.Label(m_fps, "FPS: " + m_FPS, m_Style);
            GUI.Label(m_dtime, "间隔: " + m_frameDeltaTime, m_Style);
        }
    }
}