using UnityEngine;

public enum BalloonColor { Red, Blue, Green, Yellow, Purple }

public class Balloon : MonoBehaviour
{
    public BalloonColor color;

    [Header("Optional")]
    public GameObject popEffect;   // particle prefab spawned when popped
    public AudioClip popSound;

    BalloonBoothGame booth;
    Collider[] colliders;
    Renderer[] renderers;
    bool popped = false;

    void Start()
    {
        booth = GetComponentInParent<BalloonBoothGame>();
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    // The balloon's collider must have Is Trigger ticked
    void OnTriggerEnter(Collider other)
    {
        if (popped) return;
        if (other.GetComponentInParent<Dart>() == null) return;
        Pop();
    }

    void Pop()
    {
        popped = true;
        SetVisible(false);

        if (popEffect != null) Instantiate(popEffect, transform.position, Quaternion.identity);
        if (popSound != null) AudioSource.PlayClipAtPoint(popSound, transform.position);

        if (booth != null) booth.OnBalloonPopped(this);
    }

    public void ResetState()
    {
        popped = false;
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
        foreach (var c in colliders) c.enabled = visible;
    }
}