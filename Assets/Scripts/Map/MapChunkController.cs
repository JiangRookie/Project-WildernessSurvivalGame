using UnityEngine;

public class MapChunkController : MonoBehaviour
{
    public Vector3 MapChunkCenterPos { get; private set; }

    public void InitCenter(Vector3 centerPosition) => MapChunkCenterPos = centerPosition;
}