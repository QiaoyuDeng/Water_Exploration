using System.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;

public class MRTKSceneTransition : MonoBehaviour
{
    public Transform targetView;        // Target Position
    public float duration = 2.0f;       // Transition duration
    public FarmIntroSequence farmIntro;

    [Header("Audio Introduction")]          
    public AudioSource audioSource;
    public AudioClip introAudioClips;

    public void BeginTransition()
    {
        Debug.Log($"[DEBUG] This GameObject name: {gameObject.name}");
        StartCoroutine(TransitionToTarget());
    }

    private IEnumerator TransitionToTarget()
    {
        // Step 1: Reset the main camera to zero to prevent the user's perspective from affecting the position offset
        Transform mainCamera = Camera.main.transform;
        mainCamera.localPosition = Vector3.zero;
        mainCamera.localRotation = Quaternion.identity;

        // Step 2: Start the transition animation
        Transform playspace = MixedRealityPlayspace.Transform;

        Vector3 startPos = playspace.position;
        Quaternion startRot = playspace.rotation;

        Vector3 endPos = targetView.position;
        Quaternion endRot = targetView.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            playspace.position = Vector3.Lerp(startPos, endPos, t);
            playspace.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Step 3: Ensure the final position
        playspace.position = endPos;
        playspace.rotation = endRot;

        if (farmIntro != null)
        {
            farmIntro.StartFarmIntro();
        }


        Debug.Log($"audioSource is null: {audioSource == null}");
        Debug.Log($"introAudioClips is null: {introAudioClips == null}");
        if (audioSource != null && introAudioClips != null)
        {
            Debug.Log("Playing audio clip...");
            audioSource.clip = introAudioClips;
            audioSource.Play();
        }

    }

}
