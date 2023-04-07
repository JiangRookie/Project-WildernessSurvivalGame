using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MapObjectBase
{
    [SerializeField] protected Collider m_Collider;
    public Collider Collider => m_Collider;

    #region PreviewMode

    static Color s_Red = new Color(1, 0, 0, 0.5f);
    static Color s_Green = new Color(0, 1, 0, 0.5f);
    List<Material> m_MaterialList = null;

    public virtual void InitOnPreview()
    {
        m_Collider.enabled = false;
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        m_MaterialList = new List<Material>(10);
        foreach (var meshRenderer in meshRenderers)
        {
            m_MaterialList.AddRange(meshRenderer.materials);
        }
        foreach (var material in m_MaterialList)
        {
            material.color = s_Red;
            ProjectTool.SetMaterialRenderingMode(material, ProjectTool.RenderingMode.Fade);
        }
    }

    public void SetColorOnPreview(bool isRed)
    {
        foreach (var material in m_MaterialList)
        {
            material.color = isRed ? s_Red : s_Green;
        }
    }

    #endregion
}