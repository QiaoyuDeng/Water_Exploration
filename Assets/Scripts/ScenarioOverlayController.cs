using System.Collections;
using UnityEngine;
using TMPro;

public class ScenarioOverlayController : MonoBehaviour
{
    public CanvasGroup overlayGroup;            // Drag the CanvasGroup from the panel
    public TextMeshProUGUI scenarioText;        // Drag the TMP Text component
    public AudioSource audioSource;             // Play audio when the overlay is shown

    public float fadeDuration = 1.5f;           // Duration for fade in/out
    //public float showTime = 2f;                 // Duration to show the text


    [Header("Testing Options")]
    public bool skipAudio = false; // If true, override clip length to 1s for testing purposes


    public IEnumerator ShowScenarioText(string textToShow, AudioClip clip)
    {
        float showTime = skipAudio ? 1f : clip.length;


        // Make sure the GameObject is active
        gameObject.SetActive(true);
        Debug.Log("Overlay GameObject Activated");
        overlayGroup.alpha = 0f;
        scenarioText.text = textToShow;

        Debug.Log($"Will play clip: {(clip != null ? clip.name : "null")}");
        Debug.Log($"Playing clip: {clip.name}, duration: {clip.length}s");
        if (!skipAudio && audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or Clip is null");
        }


        // Fade in
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            overlayGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        overlayGroup.alpha = 1f;
         
        // Wait before fading out
        yield return new WaitForSeconds(showTime);

        // Fade out
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            overlayGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        overlayGroup.alpha = 0f;

        // ✅ Hide the overlay panel after fade-out
        gameObject.SetActive(false);
    }
}
