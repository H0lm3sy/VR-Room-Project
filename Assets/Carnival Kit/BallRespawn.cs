using UnityEngine;

public class BallRespawn : MonoBehaviour
{
    public float killHeight = -5f; // respawn if the ball falls below this world Y

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
        rb.linearVelocity = Vector3.zero;   // Unity 2022 or older: rb.velocity
        rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(startPos, startRot);
    }
}