using System.Collections;
using JKFrame;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UITool
{
    /// <summary>
    /// Binding mouse effects（Add mouse effects）
    /// </summary>
    /// <param name="component">Components that require the use of mouse effects</param>
    public static void BindMouseEffect(this Component component)
    {
        var localScale = component.transform.localScale;
        component.OnMouseEnter(MouseEffect, component, true, localScale);
        component.OnMouseExit(MouseEffect, component, false, localScale);
    }

    /// <summary>
    /// Remove mouse effects
    /// </summary>
    /// <param name="component">The component that needs to remove the mouse effect</param>
    public static void RemoveMouseEffect(this Component component)
    {
        // 手动触发一次退出
        var listener = component.GetComponent<JKEventListener>();
        if (listener != null) listener.OnPointerExit(null);

        component.RemoveMouseEnter(MouseEffect);
        component.RemoveMouseExit(MouseEffect);

        GameManager.Instance.SetCursorState(CursorState.Normal);
    }

    static void MouseEffect(PointerEventData arg1, object[] arg2)
    {
        Component component = (Component)arg2[0];
        bool useEffect = (bool)arg2[1];
        Vector3 originScale = (Vector3)arg2[2];

        // 设置鼠标指针的外观
        GameManager.Instance.SetCursorState(useEffect ? CursorState.Handle : CursorState.Normal);
        component.StartCoroutine(MouseEffectCoroutine(component, useEffect, originScale));
    }

    /// <summary>
    /// 鼠标特效协程
    /// </summary>
    /// <param name="component">Components that require the use of mouse effects</param>
    /// <param name="useEffect"></param>
    /// <param name="originScale">Origin scale</param>
    /// <returns></returns>
    static IEnumerator MouseEffectCoroutine(Component component, bool useEffect, Vector3 originScale)
    {
        Transform transform = component.transform;
        Vector3 currScale = transform.localScale;
        Vector3 targetScale;
        if (useEffect) // 放大
        {
            targetScale = originScale * 1.1f;
            while (transform.localScale.x < targetScale.x)
            {
                yield return null;
                if (component == null) yield break;

                currScale += Time.deltaTime * 2 * Vector3.one;
                transform.localScale = currScale;
            }
            transform.localScale = targetScale;
        }
        else // 恢复初始大小
        {
            targetScale = originScale;
            while (transform.localScale.x > targetScale.x)
            {
                yield return null;
                if (component == null) yield break;

                currScale -= Time.deltaTime * 2 * Vector3.one;
                transform.localScale = currScale;
            }
            transform.localScale = targetScale;
        }
    }
}