using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        transform.Translate(
            Time.deltaTime * 3
          * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
    }
}