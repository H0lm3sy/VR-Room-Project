using System.Collections;
using UnityEngine;
using TMPro;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// XRI 2.x: delete the two lines above and use this one instead
// using UnityEngine.XR.Interaction.Toolkit;

public class BoothGameBottle : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    [Tooltip("Drag the tennis balls here from the Hierarchy")]
    public Transform[] ballObjects;

    class SavedState
    {
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Rigidbody rb;
        public BottleScript bottle;
        public XRGrabInteractable grab;
        public XRSocketInteractor socket; // the socket this ball started in
    }

    SavedState[] items;
    int score = 0;

    void Start()
    {
        var bottles = GetComponentsInChildren<BottleScript>(true);
        var sockets = GetComponentsInChildren<XRSocketInteractor>(true);

        items = new SavedState[bottles.Length + ballObjects.Length];
        int i = 0;
        foreach (var b in bottles) items[i++] = Save(b.transform, sockets, false);
        foreach (var b in ballObjects) items[i++] = Save(b, sockets, true);

        Debug.Log("Bottles: " + bottles.Length + " | Balls: " + ballObjects.Length);
        UpdateText();
    }

    SavedState Save(Transform t, XRSocketInteractor[] sockets, bool isBall)
    {
        var grab = t.GetComponent<XRGrabInteractable>();
        XRSocketInteractor socket = null;

        if (isBall && grab != null)
        {
            // Use the socket currently holding the ball, if any
            socket = grab.firstInteractorSelecting as XRSocketInteractor;

            // Otherwise fall back to the nearest socket within 30 cm
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

        return new SavedState
        {
            t = t,
            pos = t.position,
            rot = t.rotation,
            rb = t.GetComponent<Rigidbody>(),
            bottle = t.GetComponent<BottleScript>(),
            grab = grab,
            socket = socket
        };
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateText();
    }

    // Hook this up to the restart button's event
    public void ResetGame()
    {
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        // 1. Release anything held by a hand or socket
        foreach (var s in items)
        {
            if (s.grab != null && s.grab.isSelected)
                s.grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)s.grab);
        }

        yield return null; // let the interaction system finish releasing

        // 2. Move everything back to its starting spot
        foreach (var s in items)
        {
            if (s.rb != null && !s.rb.isKinematic)
            {
                s.rb.linearVelocity = Vector3.zero;   // Unity 2022 or older: velocity
                s.rb.angularVelocity = Vector3.zero;
            }

            s.t.SetPositionAndRotation(s.pos, s.rot);

            if (s.rb != null && !s.rb.isKinematic) s.rb.Sleep();
            if (s.bottle != null) s.bottle.ResetState();
        }

        yield return null;

        // 3. Put balls back into their sockets
        foreach (var s in items)
        {
            if (s.grab != null && s.socket != null)
                s.socket.interactionManager.SelectEnter(s.socket, (IXRSelectInteractable)s.grab);
        }

        score = 0;
        UpdateText();
    }

    void UpdateText()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }
}