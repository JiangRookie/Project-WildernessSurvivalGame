using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        transform.Translate
            (new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxis("Vertical") * Time.deltaTime * 4));
    }
}