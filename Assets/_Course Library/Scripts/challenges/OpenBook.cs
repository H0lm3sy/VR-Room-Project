using UnityEngine;

public class OpenBook : MonoBehaviour
{
    public GameObject Cover;
    public HingeJoint myHinge;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var myHinge = Cover.GetComponent<HingeJoint>();

        myHinge.useMotor = false;

    }

    public void OpenSesame()
    {
        myHinge.useMotor = true;
    }
}
