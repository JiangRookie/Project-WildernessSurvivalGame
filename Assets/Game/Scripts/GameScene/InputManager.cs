using System.Collections.Generic;
using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : SingletonMono<InputManager>
{
    [SerializeField] LayerMask m_MapObjectLayer;
    bool m_NeedToCheck = false;
    List<RaycastResult> m_RaycastResultList = new();

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        if (m_NeedToCheck == false) return;
        CheckSelectMapObject();
    }

    public void Init()
    {
        SetCheckState(true);
    }

    public void SetCheckState(bool needToCheck)
    {
        m_NeedToCheck = needToCheck;
    }

    void CheckSelectMapObject()
    {
        if (Input.GetMouseButton(0))
        {
            // 如果检测到UI则无视
            if (CheckMouseOnUI()) return;

            Ray ray = CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, m_MapObjectLayer))
            {
                // 发给PlayerController去处理
                PlayerController.Instance.OnSelectMapObject(hitInfo);
            }
        }
    }

    /// <summary>
    /// 检查鼠标是否在UI上
    /// </summary>
    public bool CheckMouseOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        // 射线检测除了Mast外是否有其他UI物体
        EventSystem.current.RaycastAll(pointerEventData, m_RaycastResultList);
        for (int i = 0; i < m_RaycastResultList.Count; i++)
        {
            RaycastResult raycastResult = m_RaycastResultList[i];

            // 是UI同时不是Mast作用的物体
            if (raycastResult.gameObject.GetComponent<RectTransform>()
             && raycastResult.gameObject.name != "Mask")
            {
                m_RaycastResultList.Clear();
                return true;
            }
        }
        m_RaycastResultList.Clear();
        return false;
    }
}