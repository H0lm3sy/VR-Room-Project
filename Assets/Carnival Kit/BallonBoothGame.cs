using System;
using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// XRI 2.x: delete the two lines above and use this one instead
// using UnityEngine.XR.Interaction.Toolkit;

public class BalloonBoothGame : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public int popsToWin = 3;

    [Tooltip("Drag the darts here from the Hierarchy")]
    public Transform[] dartObjects;

    class DartState
    {
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Rigidbody rb;
        public XRGrabInteractable grab;
        public XRSocketInteractor socket;
    }

    Balloon[] balloons;
    DartState[] darts;
    int[] counts;
    bool won = false;

    void Start()
    {
        balloons = GetComponentsInChildren<Balloon>(true);
        counts = new int[Enum.GetValues(typeof(BalloonColor)).Length];

        var sockets = GetComponentsInChildren<XRSocketInteractor>(true);
        darts = new DartState[dartObjects.Length];
        for (int i = 0; i < dartObjects.Length; i++)
            darts[i] = SaveDart(dartObjects[i], sockets);

        Debug.Log("Balloons: " + balloons.Length + " | Darts: " + dartObjects.Length);
        UpdateText();
    }

    DartState SaveDart(Transform t, XRSocketInteractor[] sockets)
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

        return new DartState
        {
            t = t,
            pos = t.position,
            rot = t.rotation,
            rb = t.GetComponent<Rigidbody>(),
            grab = grab,
            socket = socket
        };
    }

    public void OnBalloonPopped(Balloon balloon)
    {
        if (won) return;

        int c = (int)balloon.color;
        counts[c]++;

        if (counts[c] >= popsToWin)
        {
            won = true;
            if (statusText != null)
                statusText.text = "YOU WIN!\n" + popsToWin + " " + balloon.color + " balloons";
            return;
        }

        UpdateText();
    }

    // Hook this up to the restart button's event
    public void ResetGame()
    {
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        // 1. Release any dart held by a hand or socket
        foreach (var d in darts)
        {
            if (d.grab != null && d.grab.isSelected)
                d.grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)d.grab);
        }

        yield return null;

        // 2. Bring balloons back and move darts to their start spots
        foreach (var b in balloons) b.ResetState();

        foreach (var d in darts)
        {
            if (d.rb != null && !d.rb.isKinematic)
            {
                d.rb.linearVelocity = Vector3.zero;   // Unity 2022 or older: velocity
                d.rb.angularVelocity = Vector3.zero;
            }
            d.t.SetPositionAndRotation(d.pos, d.rot);
            if (d.rb != null && !d.rb.isKinematic) d.rb.Sleep();
        }

        yield return null;

        // 3. Put darts back into their sockets
        foreach (var d in darts)
        {
            if (d.grab != null && d.socket != null)
                d.socket.interactionManager.SelectEnter(d.socket, (IXRSelectInteractable)d.grab);
        }

        won = false;
        Array.Clear(counts, 0, counts.Length);
        UpdateText();
    }

    void UpdateText()
    {
        if (statusText == null) return;

        var sb = new StringBuilder("Pop " + popsToWin + " of one colour!");
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] > 0)
                sb.Append("\n" + (BalloonColor)i + ": " + counts[i] + "/" + popsToWin);
        }
        statusText.text = sb.ToString();
    }
}