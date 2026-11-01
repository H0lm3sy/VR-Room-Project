using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// XRI 3.x only, add these two lines:
// using UnityEngine.XR.Interaction.Toolkit.Interactables;
// using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;

public class HatTooltip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject tooltipCanvas;
    [SerializeField] TMP_Text messageText;

    [Header("Settings")]
    [SerializeField] float showDistance = 1.5f;
    [SerializeField] Vector3 offset = new Vector3(0, 0.3f, 0);

    [Header("Messages")]
    [SerializeField] string pickUpMessage = "Grab the hat\n(hold the Grip button)";
    [SerializeField] string wearMessage = "Now put it on your head!";

    const string PrefKey = "Tooltip_Hat_Completed";

    enum Stage { Hidden, PickUp, PutOnHead, Done }

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    Transform cam;
    Stage current = Stage.Hidden;
    bool isHeld;
    bool isWorn;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        cam = Camera.main.transform;
    }

    void Start()
    {
        // First-time-only: skip everything if already completed
        if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            enabled = false;
            tooltipCanvas.SetActive(false);
            return;
        }
        SetStage(Stage.Hidden);
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        // A socket is also an "interactor", so check which one grabbed us
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            isWorn = true;
        else
            isHeld = true;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            isWorn = false;
        else
            isHeld = false;
    }

    void Update()
    {
        // Decide which stage we should be in
        Stage next;
        if (isWorn)
            next = Stage.Done;
        else if (isHeld)
            next = Stage.PutOnHead;
        else if (Vector3.Distance(cam.position, transform.position) < showDistance)
            next = Stage.PickUp;
        else
            next = Stage.Hidden;

        if (next != current)
            SetStage(next);
    }

    void LateUpdate()
    {
        // Keep the tooltip floating above the hat
        if (tooltipCanvas.activeSelf)
            tooltipCanvas.transform.position = transform.position + offset;
    }

    void SetStage(Stage stage)
    {
        current = stage;

        switch (stage)
        {
            case Stage.Hidden:
                tooltipCanvas.SetActive(false);
                break;

            case Stage.PickUp:
                messageText.text = pickUpMessage;
                tooltipCanvas.SetActive(true);
                break;

            case Stage.PutOnHead:
                messageText.text = wearMessage;
                tooltipCanvas.SetActive(true);
                break;

            case Stage.Done:
                tooltipCanvas.SetActive(false);
                PlayerPrefs.SetInt(PrefKey, 1);
                PlayerPrefs.Save();
                enabled = false; // stop checking
                break;
        }
    }

    [ContextMenu("Reset Tooltip Progress")]
    void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        Debug.Log("Hat tooltip progress reset.");
    }
}