using UnityEngine;

public class BounceSound : MonoBehaviour
{
    public AudioClip bounceClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        audioSource.PlayOneShot(bounceClip);
    }
}
