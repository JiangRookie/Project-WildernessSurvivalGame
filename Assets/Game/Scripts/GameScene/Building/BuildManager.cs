using System.Collections;
using System.Collections.Generic;
using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public class BuildManager : SingletonMono<BuildManager>
{
    [SerializeField] float m_VirtualCellSize = 0.25f;
    Dictionary<string, BuildingBase> m_BuildingPreviewGameObjDict = new Dictionary<string, BuildingBase>();
    [SerializeField] LayerMask m_BuildLayerMask;

    public void Init()
    {
        UIManager.Instance.Show<UI_BuildWindow>();
        EventManager.AddEventListener<BuildConfig>(EventName.BuildBuilding, BuildBuilding);
    }

    void BuildBuilding(BuildConfig buildConfig)
    {
        StartCoroutine(DoBuildBuilding(buildConfig));
    }

    IEnumerator DoBuildBuilding(BuildConfig buildConfig)
    {
        UIManager.Instance.DisableUIGraphicRaycaster(); // 关闭UI交互

        // 进入建造状态
        InputManager.Instance.SetCheckState(false);

        // 生成预览物体
        GameObject prefab = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).Prefab;
        if (m_BuildingPreviewGameObjDict.TryGetValue(prefab.name, out BuildingBase previewBuilding))
        {
            previewBuilding.gameObject.SetActive(true);
        }
        else
        {
            previewBuilding = Instantiate(prefab, transform).GetComponent<BuildingBase>();
            previewBuilding.InitOnPreview();
            m_BuildingPreviewGameObjDict.Add(prefab.name, previewBuilding);
        }

        while (true)
        {
            // 取消建造
            if (Input.GetMouseButtonDown(1))
            {
                previewBuilding.gameObject.SetActive(false);
                UIManager.Instance.EnableUIGraphicRaycaster();
                InputManager.Instance.SetCheckState(true);
                yield break;
            }

            // 预览物体跟随鼠标
            if (InputManager.Instance.GetMouseWorldPosOnGround(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                // 把鼠标坐标转换为虚拟格子的坐标
                Vector3 virtualCellPos = mouseWorldPos;

                // Mathf.RoundToInt(mouseWorldPos.x / m_VirtualCellSize) 代表第几个格子
                virtualCellPos.x = Mathf.RoundToInt(mouseWorldPos.x / m_VirtualCellSize) * m_VirtualCellSize;
                virtualCellPos.z = Mathf.RoundToInt(mouseWorldPos.z / m_VirtualCellSize) * m_VirtualCellSize;

                previewBuilding.transform.position = virtualCellPos;
            }

            bool isOverlap = true;

            // 碰撞检测
            if (previewBuilding.Collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)previewBuilding.Collider;
                isOverlap = Physics.CheckBox
                    (boxCollider.transform.position + boxCollider.center, boxCollider.size / 2, transform.rotation, m_BuildLayerMask);
            }
            else if (previewBuilding.Collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = (CapsuleCollider)previewBuilding.Collider;
                Vector3 colliderCenterPos = capsuleCollider.transform.position + capsuleCollider.center;
                Vector3 startPos = colliderCenterPos;
                Vector3 endPos = colliderCenterPos;
                startPos.y = colliderCenterPos.y - capsuleCollider.height / 2 + capsuleCollider.radius;
                endPos.y = colliderCenterPos.y + capsuleCollider.height / 2 - capsuleCollider.radius;
                isOverlap = Physics.CheckCapsule(startPos, endPos, capsuleCollider.radius, m_BuildLayerMask);
            }
            else if (previewBuilding.Collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)previewBuilding.Collider;
                isOverlap = Physics.CheckSphere(sphereCollider.transform.position + sphereCollider.center, sphereCollider.radius, m_BuildLayerMask);
            }

            // 如果可以建在 材质球为绿色 否则为红色
            if (isOverlap)
            {
                previewBuilding.SetColorOnPreview(true);
            }
            else
            {
                previewBuilding.SetColorOnPreview(false);

                // 确定建造 根据配置扣除物资
                if (Input.GetMouseButtonDown(0))
                {
                    previewBuilding.gameObject.SetActive(false);
                    UIManager.Instance.EnableUIGraphicRaycaster();
                    InputManager.Instance.SetCheckState(true);

                    // 放置建筑物
                    MapManager.Instance.SpawnMapObject(buildConfig.TargetID, previewBuilding.transform.position);

                    // 物资的消耗
                    UI_InventoryWindow.Instance.UpdateItemsForBuild(buildConfig);
                    yield break;
                }
            }

            yield return null;
        }
    }
}