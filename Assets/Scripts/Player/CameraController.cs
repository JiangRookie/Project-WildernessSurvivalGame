using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public class CameraController : SingletonMono<CameraController>
{
    Transform m_Transform;
    [SerializeField] Transform TargetTrans;
    [SerializeField] Vector3 Offset;
    [SerializeField] float MoveSpeed;

    Vector2 m_PositionXScope;
    Vector2 m_PositionZScope;

    void Start()
    {
        Init();
    }

    void LateUpdate()
    {
        if (TargetTrans != null)
        {
            var targetPos = TargetTrans.position + Offset;
            targetPos.x = Mathf.Clamp(targetPos.x, m_PositionXScope.x, m_PositionXScope.y);
            targetPos.z = Mathf.Clamp(targetPos.z, m_PositionZScope.x, m_PositionZScope.y);
            m_Transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * MoveSpeed);
        }
    }

    void Init()
    {
        m_Transform = transform;
        InitPositionScope(MapManager.Instance.MapSizeOnWorld);
    }

    /// <summary>
    /// 初始化坐标范围
    /// </summary>
    /// <param name="mapSizeOnWorld"></param>
    void InitPositionScope(float mapSizeOnWorld)
    {
        m_PositionXScope = new Vector2(5, mapSizeOnWorld - 5);
        m_PositionZScope = new Vector2(-1, mapSizeOnWorld - 10);
    }
}