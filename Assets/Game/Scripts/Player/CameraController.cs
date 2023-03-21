using JKFrame;
using UnityEngine;

public class CameraController : SingletonMono<CameraController>
{
    Transform m_Transform;

    [SerializeField] Transform m_TargetTrans;
    [SerializeField] Vector3 m_Offset;
    [SerializeField] float m_MoveSpeed;

    Vector2 m_PositionXScope;
    Vector2 m_PositionZScope;

    void LateUpdate()
    {
        if (m_TargetTrans != null)
        {
            var targetPos = m_TargetTrans.position + m_Offset;
            targetPos.x = Mathf.Clamp(targetPos.x, m_PositionXScope.x, m_PositionXScope.y);
            targetPos.z = Mathf.Clamp(targetPos.z, m_PositionZScope.x, m_PositionZScope.y);
            m_Transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_MoveSpeed);
        }
    }

    public void Init(float mapSizeOnWorld)
    {
        m_Transform = transform;
        InitPositionScope(mapSizeOnWorld);
    }

    /// <summary>
    /// 初始化坐标范围
    /// </summary>
    /// <param name="mapSizeOnWorld"></param>
    void InitPositionScope(float mapSizeOnWorld)
    {
        m_PositionXScope = new Vector2(5, mapSizeOnWorld - 5);
        m_PositionZScope = new Vector2(-1, mapSizeOnWorld - 5);
    }
}