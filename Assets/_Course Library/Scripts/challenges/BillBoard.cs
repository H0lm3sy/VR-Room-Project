using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Transform cam;

    void Start() => cam = Camera.main.transform;

    void LateUpdate()
    {
        // Face the player, but stay upright
        Vector3 dir = transform.position - cam.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
