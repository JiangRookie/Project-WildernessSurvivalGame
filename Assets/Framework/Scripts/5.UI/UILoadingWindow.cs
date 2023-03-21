using UnityEngine;
using UnityEngine.UI;

namespace JKFrame
{
    [UIElement(true, "UI/UI_LoadingWindow", 4)]
    public class UILoadingWindow : UI_WindowBase
    {
        [SerializeField] Text m_ProgressText;
        [SerializeField] Image m_FillImage;

        public override void OnShow()
        {
            base.OnShow();
            UpdateProgress(0);
        }

        protected override void RegisterEventListener()
        {
            base.RegisterEventListener();
            EventManager.AddEventListener<float>("LoadingSceneProgress", UpdateProgress);
            EventManager.AddEventListener("LoadSceneSucceed", OnLoadSceneSucceed);
        }

        protected override void CancelEventListener()
        {
            base.CancelEventListener();
            EventManager.RemoveEventListener<float>("LoadingSceneProgress", UpdateProgress);
            EventManager.RemoveEventListener("LoadSceneSucceed", OnLoadSceneSucceed);
        }

        /// <summary>
        /// 当场景加载成功
        /// </summary>
        void OnLoadSceneSucceed() => Close();

        /// <summary>
        /// 更新进度
        /// </summary>
        void UpdateProgress(float progressValue)
        {
            m_ProgressText.text = (int)(progressValue * 100) + "%";
            m_FillImage.fillAmount = (int)(progressValue * 100);
        }
    }
}