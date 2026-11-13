using UnityEngine;

public class BottleScript : MonoBehaviour
{
    public float tipAngle = 45f;      // how far it must lean to count as knocked over
    public float dropDistance = 0.3f; // how far below its strt height counts as fallen off

    BoothGameBottle booth;
    float startY;
    bool counted = false;

    void Start()
    {
        booth = GetComponentInParent<BoothGameBottle>();
        startY = transform.position.y;
    }

    void Update()
    {
        if (counted || booth == null) return;

        float angle = Vector3.Angle(transform.up, Vector3.up);
        bool fellOff = transform.position.y < startY - dropDistance;

        if (angle > tipAngle || fellOff)
        {
            counted = true;
            booth.AddScore(1);
        }
    }

    public void ResetState()
    {
        counted = false;
    }
}