using System.Collections;
using UnityEngine;

public static class CoroutineTool
{
    public static IEnumerator WaitForSeconds(float time)
    {
        float currTime = 0;
        while (currTime < time)
        {
            currTime += Time.deltaTime;
            yield return null;
        }
    }
}