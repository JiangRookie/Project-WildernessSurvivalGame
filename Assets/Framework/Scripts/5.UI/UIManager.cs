using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JKFrame
{
    public class UIManager : ManagerBase<UIManager>
    {
        #region 内部类

        [Serializable]
        class UILayer
        {
            public Transform Root;
            public bool EnableMask;
            public Image MaskImage;
            int m_Count = 0;

            public void OnShow()
            {
                m_Count += 1;
                Update();
            }

            public void OnClose()
            {
                m_Count -= 1;
                Update();
            }

            void Update()
            {
                if (EnableMask == false) return;
                MaskImage.raycastTarget = m_Count != 0;
                int posIndex = Root.childCount - 2;
                MaskImage.transform.SetSiblingIndex(posIndex < 0 ? 0 : posIndex);
            }
        }

        #endregion

        /// <summary>
        /// 元素资源库
        /// </summary>
        public Dictionary<Type, UIElement> UIElementDic => GameRoot.Instance.GameSetting.UIElementDic;

        [SerializeField] UILayer[] m_UILayers;
        [SerializeField] UITips m_UITips; // 提示窗
        [SerializeField] GraphicRaycaster m_GraphicRaycaster;
        const string TipsLocalizationPackName = "Tips";
        [SerializeField] RectTransform m_DragLayer;
        public RectTransform DragLayer => m_DragLayer;

        public void AddTips(string info) => m_UITips.AddTips(info);

        public void AddTipsByLocalization(string tipsKeyName)
        {
            m_UITips.AddTips
            (
                LocalizationManager.Instance.GetContent<L_Text>(TipsLocalizationPackName, tipsKeyName).content
            );
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <param name="layer">层级 -1等于不设置</param>
        public T Show<T>(int layer = -1) where T : UI_WindowBase
        {
            return Show(typeof(T), layer) as T;
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="type">窗口类型</param>
        /// <param name="layer">层级 -1等于不设置</param>
        public UI_WindowBase Show(Type type, int layer = -1)
        {
            if (UIElementDic.ContainsKey(type))
            {
                UIElement info = UIElementDic[type];
                int layerNum = layer == -1 ? info.layerNum : layer;

                // 实例化实例或者获取到实例，保证窗口实例存在
                if (info.objInstance != null)
                {
                    info.objInstance.Show();
                    info.objInstance.transform.SetParent(m_UILayers[layerNum].Root);
                    info.objInstance.transform.SetAsLastSibling();
                    info.objInstance.OnShow();
                }
                else
                {
                    UI_WindowBase window = ResManager.InstantiateForPrefab(info.prefab, m_UILayers[layerNum].Root).GetComponent<UI_WindowBase>();
                    info.objInstance = window;
                    window.OnInit();
                    window.OnShow();
                }
                info.layerNum = layerNum;
                m_UILayers[layerNum].OnShow();
                return info.objInstance;
            }

            // 资源库中没有意味着不允许显示
            return null;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        public void Close<T>() => Close(typeof(T));

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="type">窗口类型</param>
        public void Close(Type type)
        {
            if (UIElementDic.ContainsKey(type))
            {
                UIElement info = UIElementDic[type];
                if (info.objInstance == null) return;
                info.objInstance.OnClose();

                // 缓存则隐藏
                if (info.isCache)
                {
                    info.objInstance.transform.SetAsFirstSibling();
                    info.objInstance.Hide();
                }

                // 不缓存则销毁
                else
                {
                    Destroy(info.objInstance.gameObject);
                    info.objInstance = null;
                }
                m_UILayers[info.layerNum].OnClose();
            }
        }

        /// <summary>
        /// 关闭全部窗口
        /// </summary>
        public void CloseAll()
        {
            // 处理缓存中所有状态的逻辑
            var enumerator = UIElementDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.objInstance != null
                 && enumerator.Current.Value.objInstance.gameObject.activeInHierarchy == true)
                {
                    enumerator.Current.Value.objInstance.Close();
                }
            }
        }

        public void CloseAllDisposed()
        {
            // 处理缓存中所有状态的逻辑
            using var enumerator = UIElementDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.objInstance != null
                 && enumerator.Current.Value.objInstance.gameObject.activeInHierarchy == true)
                {
                    enumerator.Current.Value.objInstance.Close();
                }
            }
        }

        public void CloseAllForeach()
        {
            foreach (var value in UIElementDic.Values)
            {
                if (value.objInstance != null && value.objInstance.gameObject.activeInHierarchy)
                {
                    value.objInstance.Close();
                }
            }
        }

        public void CloseAllLinq()
        {
            foreach
            (var uiElement
                in UIElementDic.Values
                               .Where(uiElement => uiElement.objInstance != null
                                       && uiElement.objInstance.gameObject.activeInHierarchy))
            {
                uiElement.objInstance.Close();
            }
        }

        public void EnableUIGraphicRaycaster() => m_GraphicRaycaster.enabled = true;

        public void DisableUIGraphicRaycaster() => m_GraphicRaycaster.enabled = false;
    }
}