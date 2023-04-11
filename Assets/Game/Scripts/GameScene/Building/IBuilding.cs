using System.Collections.Generic;
using UnityEngine;

public interface IBuilding
{
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public static Color Red = new Color(1, 0, 0, 0.5f);
    public static Color Green = new Color(0, 1, 0, 0.5f);
    public List<Material> MaterialList { get; set; }
    public void OnPreview();

    public void InitOnPreview()
    {
        Collider.enabled = false;
        MeshRenderer[] meshRenderers = GameObject.GetComponentsInChildren<MeshRenderer>();
        MaterialList = new List<Material>(10);
        foreach (var meshRenderer in meshRenderers)
        {
            MaterialList.AddRange(meshRenderer.materials);
        }
        foreach (var material in MaterialList)
        {
            material.color = Red;
            ProjectTool.SetMaterialRenderingMode(material, ProjectTool.RenderingMode.Fade);
        }
        OnPreview();
    }

    public void SetColorOnPreview(bool isRed)
    {
        foreach (var material in MaterialList)
        {
            material.color = isRed ? Red : Green;
        }
    }
}