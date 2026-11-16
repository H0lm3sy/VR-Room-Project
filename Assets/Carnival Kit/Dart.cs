using UnityEngine;

// Marker for darts. Respawns a dart that falls out of the play area.
public class Dart : MonoBehaviour
{
    public float killHeight = -5f;

    Vector3 startPos;
    Quaternion startRot;
    Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < killHeight) Respawn();
    }

    public void Respawn()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;   // Unity 2022 or older: velocity
            rb.angularVelocity = Vector3.zero;
        }
        transform.SetPositionAndRotation(startPos, startRot);
    }
}