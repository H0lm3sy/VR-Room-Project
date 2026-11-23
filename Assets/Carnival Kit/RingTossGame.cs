using System.Collections;
using UnityEngine;
using TMPro;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// XRI 2.x: delete the two lines above and use this one instead
// using UnityEngine.XR.Interaction.Toolkit;

public class RingTossGame : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public AudioClip scoreSound;
    public AudioSource audioSource;

    [Tooltip("Drag the rings here from the Hierarchy")]
    public Transform[] ringObjects;

    class RingState
    {
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Rigidbody rb;
        public Ring ring;
        public XRGrabInteractable grab;
        public XRSocketInteractor socket;
    }

    RingState[] rings;
    int score = 0;

    void Start()
    {
        var sockets = GetComponentsInChildren<XRSocketInteractor>(true);
        rings = new RingState[ringObjects.Length];
        for (int i = 0; i < ringObjects.Length; i++)
            rings[i] = SaveRing(ringObjects[i], sockets);

        Debug.Log("Rings: " + ringObjects.Length + " | Pegs: " + GetComponentsInChildren<RingPeg>(true).Length);
        UpdateText();
    }

    RingState SaveRing(Transform t, XRSocketInteractor[] sockets)
    {
        var grab = t.GetComponent<XRGrabInteractable>();
        XRSocketInteractor socket = null;

        if (grab != null)
        {
            socket = grab.firstInteractorSelecting as XRSocketInteractor;

            // Otherwise use the nearest socket within 30 cm
            if (socket == null)
            {
                float best = 0.3f;
                foreach (var s in sockets)
                {
                    float d = Vector3.Distance(s.transform.position, t.position);
                    if (d < best) { best = d; socket = s; }
                }
            }
        }

        return new RingState
        {
            t = t,
            pos = t.position,
            rot = t.rotation,
            rb = t.GetComponent<Rigidbody>(),
            ring = t.GetComponent<Ring>(),
            grab = grab,
            socket = socket
        };
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (audioSource != null && scoreSound != null)
            audioSource.PlayOneShot(scoreSound);
        UpdateText();
        
    }

    // Hook this up to the restart button's event
    public void ResetGame()
    {
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        // 1. Release any ring held by a hand or socket
        foreach (var r in rings)
        {
            if (r.grab != null && r.grab.isSelected)
                r.grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)r.grab);
        }

        yield return null;

        // 2. Move rings back to their starting spots
        foreach (var r in rings)
        {
            if (r.rb != null && !r.rb.isKinematic)
            {
                r.rb.linearVelocity = Vector3.zero;   // Unity 2022 or older: velocity
                r.rb.angularVelocity = Vector3.zero;
            }
            r.t.SetPositionAndRotation(r.pos, r.rot);
            if (r.rb != null && !r.rb.isKinematic) r.rb.Sleep();
            if (r.ring != null) r.ring.ResetState();
        }

        yield return null;

        // 3. Put rings back into their sockets
        foreach (var r in rings)
        {
            if (r.grab != null && r.socket != null)
                r.socket.interactionManager.SelectEnter(r.socket, (IXRSelectInteractable)r.grab);
        }

        score = 0;
        UpdateText();
    }

    void UpdateText()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }
}