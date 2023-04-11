using System.Collections.Generic;
using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : SingletonMono<InputManager>
{
    [SerializeField] LayerMask m_BigMapObjectLayer;
    [SerializeField] LayerMask m_MapObjectLayerForMouseCanInteract; // Map object layer that the mouse can interact with
    [SerializeField] LayerMask m_GroundLayer;
    [SerializeField] LayerMask m_BuildingLayer;
    bool m_NeedToCheck = false;
    List<RaycastResult> m_RaycastResultList = new();

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
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
        if (m_NeedToCheck == false) return;

        bool mouseButtonDown = Input.GetMouseButtonDown(0);
        bool mouseButton = Input.GetMouseButton(0);

        if (mouseButtonDown || mouseButton)
        {
            // 如果检测到UI则无视
            if (CheckMouseOnUI()) return;

            Ray ray = CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, m_MapObjectLayerForMouseCanInteract))
            {
                // 发给PlayerController去处理
                PlayerController.Instance.OnSelectMapObject(hitInfo, mouseButtonDown);
            }

            // 处理建筑物逻辑
            if (mouseButtonDown && Physics.Raycast(ray, out hitInfo, 100, m_BuildingLayer))
            {
                BuildingBase building = hitInfo.collider.GetComponent<BuildingBase>();
                if (building.TouchDistance > 0)
                {
                    if (Vector3.Distance(PlayerController.Instance.transform.position, building.transform.position) < building.TouchDistance)
                    {
                        building.OnSelect();
                    }
                    else
                    {
                        UIManager.Instance.AddTips("离近一点");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                }
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
            if (raycastResult.gameObject.GetComponent<RectTransform>() && raycastResult.gameObject.name != "Mask")
            {
                m_RaycastResultList.Clear();
                return true;
            }
        }
        m_RaycastResultList.Clear();
        return false;
    }

    /// <summary>
    /// 获取鼠标在地面的世界坐标
    /// </summary>
    /// <returns></returns>
    public bool GetMouseWorldPosOnGround(Vector3 mousePos, out Vector3 mouseWorldPos)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hitInfo, 1000, m_GroundLayer))
        {
            mouseWorldPos = hitInfo.point;
            return true;
        }
        mouseWorldPos = Vector3.zero;
        return false;
    }

    public bool CheckMouseOnBigMapObject()
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 1000, m_BigMapObjectLayer);
    }
}