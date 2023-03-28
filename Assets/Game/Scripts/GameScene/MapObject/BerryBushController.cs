using UnityEngine;

public class BerryBushController : BushController
{
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] Material[] m_Materials; // 0 是有果子，1是没有果子

    public override int OnPickUp()
    {
        // 修改外表
        m_MeshRenderer.sharedMaterial = m_Materials[1];
        canPickUp = false;
        return canPickUpItemConfigID;
    }
}