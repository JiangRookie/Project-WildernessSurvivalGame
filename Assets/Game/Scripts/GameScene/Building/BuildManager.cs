using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class BuildManager : SingletonMono<BuildManager>
{
    [SerializeField] float m_VirtualCellSize = 0.25f;
    [SerializeField] LayerMask m_BuildLayerMask;
    Dictionary<string, IBuilding> m_BuildingPreviewGameObjDict = new Dictionary<string, IBuilding>();

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
        UIManager.Instance.DisableUIGraphicRaycaster();
        InputManager.Instance.SetCheckState(false);

        // 生成预览物体
        GameObject prefab = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).Prefab;
        if (m_BuildingPreviewGameObjDict.TryGetValue(prefab.name, out IBuilding previewBuilding))
        {
            previewBuilding.GameObject.Show();
        }
        else
        {
            previewBuilding = Instantiate(prefab, transform).GetComponent<IBuilding>();
            previewBuilding.InitOnPreview();
            m_BuildingPreviewGameObjDict.Add(prefab.name, previewBuilding);
        }

        while (true)
        {
            // 取消建造
            if (Input.GetMouseButtonDown(1))
            {
                previewBuilding.GameObject.Hide();
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

                previewBuilding.GameObject.transform.position = virtualCellPos;
            }

            bool isOverlap = true;

            switch (previewBuilding.Collider)
            {
                case BoxCollider boxCollider:
                {
                    var boxColliderTrans = boxCollider.transform;
                    isOverlap = Physics.CheckBox(
                        center: boxColliderTrans.position + boxCollider.center
                      , halfExtents: boxCollider.size / 2
                      , orientation: boxColliderTrans.rotation
                      , layerMask: m_BuildLayerMask);
                    break;
                }
                case CapsuleCollider capsuleCollider:
                {
                    float radius;
                    Vector3 colliderCenterPos = capsuleCollider.transform.position + capsuleCollider.center;
                    Vector3 startPos = colliderCenterPos;
                    Vector3 endPos = colliderCenterPos;
                    startPos.y = colliderCenterPos.y - capsuleCollider.height / 2 + capsuleCollider.radius;
                    endPos.y = colliderCenterPos.y + capsuleCollider.height / 2 - (radius = capsuleCollider.radius);
                    isOverlap = Physics.CheckCapsule(
                        start: startPos
                      , end: endPos
                      , radius: radius
                      , layerMask: m_BuildLayerMask);
                    break;
                }
                case SphereCollider sphereCollider:
                    isOverlap = Physics.CheckSphere(
                        position: sphereCollider.transform.position + sphereCollider.center
                      , radius: sphereCollider.radius
                      , layerMask: m_BuildLayerMask);
                    break;
            }

            // 如果可以建造 材质球为绿色 否则为红色
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
                    previewBuilding.GameObject.Hide();
                    UIManager.Instance.EnableUIGraphicRaycaster();
                    InputManager.Instance.SetCheckState(true);

                    // 放置建筑物
                    MapManager.Instance.SpawnMapObject(buildConfig.TargetID, previewBuilding.GameObject.transform.position, true);

                    // 物资的消耗
                    InventoryManager.Instance.UpdateMainInventoryItemsForBuild(buildConfig);
                    yield break;
                }
            }

            yield return null;
        }
    }
}