using UnityEngine;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// XRI 2.x: delete the line above and use this one instead
// using UnityEngine.XR.Interaction.Toolkit;

public class Ring : MonoBehaviour
{
    public float killHeight = -5f;

    [Tooltip("Local axis that points through the hole of the ring. Y for a ring lying flat in the model.")]
    public Vector3 holeAxis = Vector3.up;

    [HideInInspector] public bool scored = false;

    Vector3 startPos;
    Quaternion startRot;
    Rigidbody rb;
    XRGrabInteractable grab;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (transform.position.y < killHeight) Respawn();
    }

    public bool IsHeld => grab != null && grab.isSelected;

    // True if the ring is lying roughly flat (hole pointing up or down)
    public bool IsFlat(float maxAngle)
    {
        Vector3 axis = transform.TransformDirection(holeAxis);
        float a = Vector3.Angle(axis, Vector3.up);
        return Mathf.Min(a, 180f - a) <= maxAngle;
    }

    public bool IsSlow(float maxSpeed)
    {
        if (rb == null) return true;
        return rb.linearVelocity.sqrMagnitude <= maxSpeed * maxSpeed;   // Unity 2022 or older: rb.velocity
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

    public void ResetState()
    {
        scored = false;
    }
}