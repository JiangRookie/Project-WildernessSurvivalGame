using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JKFrame
{
    /// <summary>
    /// UI提示窗
    /// </summary>
    public class UITips : MonoBehaviour
    {
        [SerializeField] Text InfoText;
        [SerializeField] Animator Animator;
        Queue<string> m_TipsQueue = new Queue<string>();
        bool m_IsShow = false;

        /// <summary>
        /// 添加提示
        /// </summary>
        /// <param name="info">提示信息</param>
        public void AddTips(string info)
        {
            m_TipsQueue.Enqueue(info);
            ShowTips();
        }

        void ShowTips()
        {
            if (m_TipsQueue.Count <= 0 || m_IsShow) return;
            InfoText.text = m_TipsQueue.Dequeue();
            Animator.Play("Show", 0, 0);
        }

        #region 动画事件

        void StartTips() => m_IsShow = true;

        void EndTips()
        {
            m_IsShow = false;
            ShowTips();
        }

        #endregion
    }
}