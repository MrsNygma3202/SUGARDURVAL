using UnityEngine;

public class LookAtStinger : MonoBehaviour
{
    public float maxDistance = 15f;
    public LayerMask targetLayer;

    public AudioSource audioSource;
    public AudioClip stinger;

    private bool hasTriggered = false;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, targetLayer))
        {
            if (!hasTriggered && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(stinger);
                hasTriggered = true;
            }
        }
    }
}
