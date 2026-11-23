using System.Collections;
using UnityEngine;
using TMPro;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// XRI 2.x: delete the two lines above and use this one instead
// using UnityEngine.XR.Interaction.Toolkit;

public class ShootingBooth : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    [Tooltip("Drag the gun here from the Hierarchy")]
    public Gun gun;

    ShootingTarget[] targets;
    int score = 0;
    bool hitThisShot = false;
    bool gameOver = false;
    string lastMessage = "";

    // Gun reset state
    Transform gunT;
    Rigidbody gunRb;
    XRGrabInteractable gunGrab;
    XRSocketInteractor gunSocket;
    Vector3 gunPos;
    Quaternion gunRot;

    void Start()
    {
        targets = GetComponentsInChildren<ShootingTarget>(true);

        if (gun != null)
        {
            gun.ShotFired += OnShotFired;

            gunT = gun.transform;
            gunPos = gunT.position;
            gunRot = gunT.rotation;
            gunRb = gun.GetComponent<Rigidbody>();
            gunGrab = gun.GetComponent<XRGrabInteractable>();

            if (gunGrab != null)
            {
                gunSocket = gunGrab.firstInteractorSelecting as XRSocketInteractor;

                // Otherwise use the nearest socket within 30 cm
                if (gunSocket == null)
                {
                    float best = 0.3f;
                    foreach (var s in GetComponentsInChildren<XRSocketInteractor>(true))
                    {
                        float d = Vector3.Distance(s.transform.position, gunT.position);
                        if (d < best) { best = d; gunSocket = s; }
                    }
                }
            }
        }

        Debug.Log("Targets: " + targets.Length + " | Gun: " + (gun != null));
        UpdateText();
    }

    void OnDestroy()
    {
        if (gun != null) gun.ShotFired -= OnShotFired;
    }

    // Called by a target when a shot lands on it
    public void OnTargetHit(int points, string label)
    {
        if (gameOver) return;
        hitThisShot = true;
        score += points;
        lastMessage = label + "! +" + points;
    }

    // Called by the gun after every shot, once any hit has been counted
    void OnShotFired(int shotsLeft)
    {
        if (!hitThisShot) lastMessage = "Miss!";
        hitThisShot = false;

        if (shotsLeft <= 0) gameOver = true;
        UpdateText();
    }

    // Hook this up to the restart button's event
    public void ResetGame()
    {
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        // 1. Release the gun if it is held
        if (gunGrab != null && gunGrab.isSelected)
            gunGrab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)gunGrab);

        yield return null;

        // 2. Clear targets and move the gun back
        foreach (var t in targets) t.ResetState();

        if (gunT != null)
        {
            if (gunRb != null && !gunRb.isKinematic)
            {
                gunRb.linearVelocity = Vector3.zero;   // Unity 2022 or older: velocity
                gunRb.angularVelocity = Vector3.zero;
            }
            gunT.SetPositionAndRotation(gunPos, gunRot);
            if (gunRb != null && !gunRb.isKinematic) gunRb.Sleep();
        }

        yield return null;

        // 3. Put the gun back in its socket and refill ammo
        if (gunGrab != null && gunSocket != null)
            gunSocket.interactionManager.SelectEnter(gunSocket, (IXRSelectInteractable)gunGrab);

        if (gun != null) gun.Refill();

        score = 0;
        hitThisShot = false;
        gameOver = false;
        lastMessage = "";
        UpdateText();
    }

    void UpdateText()
    {
        if (scoreText == null) return;

        string text = gameOver
            ? "GAME OVER\nFinal score: " + score
            : "Score: " + score + "\nShots left: " + (gun != null ? gun.ShotsLeft : 0);

        if (lastMessage != "") text += "\n" + lastMessage;
        scoreText.text = text;
    }
}