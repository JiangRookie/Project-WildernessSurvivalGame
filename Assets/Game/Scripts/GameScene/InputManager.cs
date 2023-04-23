using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : SingletonMono<InputManager>
{
    [SerializeField] LayerMask m_BigMapObjectLayer;
    [SerializeField] LayerMask m_MapObjectLayerForMouseCanInteract;
    [SerializeField] LayerMask m_GroundLayer;
    [SerializeField] LayerMask m_BuildingLayer;
    bool m_NeedToCheck = false;
    List<RaycastResult> m_RaycastResultList = new();

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CheckSelectMapObject();
    }

    public void Init() => SetCheckState(true);

    public void SetCheckState(bool needToCheck) => m_NeedToCheck = needToCheck;

    /// <summary>
    /// 检查选中的地图对象
    /// </summary>
    void CheckSelectMapObject()
    {
        if (m_NeedToCheck == false) return;

        bool mouseButtonDown = Input.GetMouseButtonDown(0);
        bool mouseButton = Input.GetMouseButton(0);

        if (mouseButtonDown || mouseButton)
        {
            if (CheckMouseOnUI()) return;

            #region Check MapObjet

            Ray ray = CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, m_MapObjectLayerForMouseCanInteract))
            {
                // 发给PlayerController去处理
                PlayerController.Instance.OnSelectMapObjectOrAI(hitInfo, mouseButtonDown);
            }

            #endregion

            #region Check Building

            if (!mouseButtonDown || !Physics.Raycast(ray, out hitInfo, 100, m_BuildingLayer)) return;

            var building = hitInfo.collider.GetComponent<BuildingBase>();
            if (building.InteractiveDistance <= 0) return; // 科学机器无法交互，只能建造

            if (Vector3.Distance(PlayerController.Instance.transform.position, building.transform.position) < building.InteractiveDistance)
            {
                building.OnSelect();
            }
            else
            {
                UIManager.Instance.AddTips("离近一点");
                ProjectTool.PlayAudio(AudioType.Fail);
            }

            #endregion
        }
    }

    /// <summary>
    /// 检查鼠标是否在UI上
    /// </summary>
    public bool CheckMouseOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointerEventData, m_RaycastResultList);

        // 检测除了Mask外是否还有其他UI物体
        for (int i = 0; i < m_RaycastResultList.Count; i++)
        {
            RaycastResult raycastResult = m_RaycastResultList[i];
            if (raycastResult.gameObject.GetComponent<RectTransform>() == false || raycastResult.gameObject.name == "Mask") continue;
            m_RaycastResultList.Clear();
            return true;
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
        if (Physics.Raycast(CameraController.Instance.Camera.ScreenPointToRay(mousePos), out RaycastHit hitInfo, 1000, m_GroundLayer))
        {
            mouseWorldPos = hitInfo.point;
            return true;
        }
        mouseWorldPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 检测鼠标是否在较大的地图对象上
    /// </summary>
    /// <returns></returns>
    public bool CheckMouseOnBigMapObject() =>
        Physics.Raycast(CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition), 1000, m_BigMapObjectLayer);

    /// <summary>
    /// 当格子停止拖拽时检查是否处于建筑物上
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public bool CheckSlotEndDragOnBuilding(int itemID)
    {
        Ray ray = CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, m_BuildingLayer) == false) return false;
        
        var building = hitInfo.collider.GetComponent<BuildingBase>();
        return building.OnSlotEndDragSelect(itemID);
    }
}