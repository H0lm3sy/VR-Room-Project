using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

public class HatTooltip : MonoBehaviour
{
    [SerializeField] XRGrabInteractable hat;
    [SerializeField] GameObject tooltipRoot;   // the world-space canvas
    [SerializeField] TMP_Text label;
    [SerializeField] Transform head;           // Main Camera
    [SerializeField] float wornDistance = 0.25f;

    const string SeenKey = "HatTooltipSeen";

    enum Stage { Hidden, Grab, BringToHead, Done }
    Stage stage = Stage.Hidden;

    void Awake()
    {
        // Bonus: only show the first time
        if (PlayerPrefs.GetInt(SeenKey, 0) == 1)
        {
            tooltipRoot.SetActive(false);
            enabled = false;
            return;
        }
        SetStage(Stage.Hidden);
    }

    void OnEnable()
    {
        hat.hoverEntered.AddListener(OnHoverEnter);
        hat.hoverExited.AddListener(OnHoverExit);
        hat.selectEntered.AddListener(OnGrab);
        hat.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        hat.hoverEntered.RemoveListener(OnHoverEnter);
        hat.hoverExited.RemoveListener(OnHoverExit);
        hat.selectEntered.RemoveListener(OnGrab);
        hat.selectExited.RemoveListener(OnRelease);
    }

    void OnHoverEnter(HoverEnterEventArgs _)
    {
        if (stage == Stage.Hidden) SetStage(Stage.Grab);
    }

    void OnHoverExit(HoverExitEventArgs _)
    {
        if (stage == Stage.Grab && !hat.isSelected) SetStage(Stage.Hidden);
    }

    void OnGrab(SelectEnterEventArgs _)
    {
        if (stage != Stage.Done) SetStage(Stage.BringToHead);
    }

    void OnRelease(SelectExitEventArgs _)
    {
        if (stage == Stage.Done) return;

        if (IsOnHead())
            StartCoroutine(Complete());
        else
            SetStage(Stage.Hidden); // dropped it elsewhere
    }

    bool IsOnHead()
    {
        // Hat roughly at head height and close to the camera
        return Vector3.Distance(hat.transform.position, head.position) < wornDistance;
    }

    IEnumerator Complete()
    {
        stage = Stage.Done;
        tooltipRoot.SetActive(true);
        label.text = "Nice hat!";
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        yield return new WaitForSeconds(2f);
        tooltipRoot.SetActive(false);
        enabled = false;
    }

    void SetStage(Stage s)
    {
        stage = s;
        switch (s)
        {
            case Stage.Hidden:
                tooltipRoot.SetActive(false);
                break;
            case Stage.Grab:
                tooltipRoot.SetActive(true);
                label.text = "Squeeze the grip button to pick up the hat";
                break;
            case Stage.BringToHead:
                tooltipRoot.SetActive(true);
                label.text = "Now bring it to your head and let go";
                break;
        }
    }

    void LateUpdate()
    {
        // Billboard: make the tooltip face the player
        if (tooltipRoot.activeSelf)
        {
            Vector3 dir = tooltipRoot.transform.position - head.position;
            tooltipRoot.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}