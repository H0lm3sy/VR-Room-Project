using UnityEngine;
using TMPro;

// XRI 3.x and 2.x
using UnityEngine.XR.Interaction.Toolkit;
// XRI 3.x only: delete this line if you are on XRI 2.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Gun : MonoBehaviour
{
    [Header("Shooting")]
    [Tooltip("Empty child at the tip of the barrel. Its blue (Z) arrow must point the way the gun shoots.")]
    public Transform muzzle;
    public float range = 20f;
    public LayerMask hitMask = ~0;
    public float fireCooldown = 0.2f;
    public float hitForce = 2f;   // small push on physics objects that get hit

    [Header("Ammo")]
    public int maxAmmo = 3;
    public TextMeshProUGUI ammoText;   // optional

    [Header("Effects (all optional)")]
    public AudioSource audioSource;
    public AudioClip shotSound;
    public AudioClip emptySound;
    public GameObject muzzleFlash;
    public float flashTime = 0.05f;

    XRGrabInteractable grab;
    int ammo;
    float nextFire;

    public int ShotsLeft => ammo;

    // Raised after every shot, with the ammo that is left
    public event System.Action<int> ShotFired;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        ammo = maxAmmo;
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
        UpdateText();
    }

    void OnEnable() { grab.activated.AddListener(OnActivated); }
    void OnDisable() { grab.activated.RemoveListener(OnActivated); }

    void OnActivated(ActivateEventArgs args)
    {
        Fire();
    }

    void Fire()
    {
        if (Time.time < nextFire) return;
        nextFire = Time.time + fireCooldown;

        if (ammo <= 0)
        {
            Play(emptySound);
            return;
        }

        ammo--;
        UpdateText();
        Play(shotSound);

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            CancelInvoke(nameof(HideFlash));
            Invoke(nameof(HideFlash), flashTime);
        }

        Transform m = muzzle != null ? muzzle : transform;
        var ray = new Ray(m.position, m.forward);

        // Debug: green line = full shot path (visible in the Scene view and in Game view with Gizmos on)
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green, 3f);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            var target = hit.collider.GetComponentInParent<ShootingTarget>();

            // Debug: red line = where the shot stopped
            Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);
            Debug.Log("Shot hit: " + hit.collider.name + " | has ShootingTarget: " + (target != null));

            if (target != null) target.Hit(hit.point, hit.normal);

            if (hit.rigidbody != null)
                hit.rigidbody.AddForceAtPosition(ray.direction * hitForce, hit.point, ForceMode.Impulse);
        }
        else
        {
            Debug.Log("Shot hit nothing");
        }

        ShotFired?.Invoke(ammo);
    }

    public void Refill()
    {
        ammo = maxAmmo;
        UpdateText();
    }

    void HideFlash()
    {
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    void UpdateText()
    {
        if (ammoText != null) ammoText.text = "Ammo: " + ammo;
    }
}