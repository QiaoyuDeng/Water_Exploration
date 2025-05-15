using UnityEngine;

public class VoiceTrigger : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip narrationClip;

    private bool hasPlayed = false; 
    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            if (audioSource != null && narrationClip != null)
            {
                audioSource.clip = narrationClip;
                audioSource.Play();
                hasPlayed = true;
                Debug.Log("Narration started after reaching trigger zone!");
            }
        }
    }
}
