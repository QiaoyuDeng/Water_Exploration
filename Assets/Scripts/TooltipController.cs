using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;


public class TooltipController : MonoBehaviour
{
    public GameObject[] tooltips; // Store all Tooltip objects
    public AudioClip[] audioClips; // Store audio files
    public AudioSource audioSource; // Audio player
    private bool isRunning = false;
    public GameObject startBoard;
    public GameObject dialogPrefab;
    public MenuIntroController MenuIntroController;
    public MRTKSceneTransition sceneTransition;
    public GameObject sceneTransitionObj;

    [Header("Testing Options")]
    public bool skipAudio = false;

    public GameObject clickableObj;
    public GameObject scene3Timer;


    private void Start()
    {
        foreach (GameObject tooltip in tooltips)
        {
            tooltip.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void StartTooltipSequence()
    {
        startBoard.SetActive(false);
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(ShowTooltipsSequentially());
        }
    }

    private IEnumerator ShowTooltipsSequentially()
    {
        foreach (GameObject tooltip in tooltips)
        {
            tooltip.SetActive(false);
        }

        for (int i = 0; i < tooltips.Length; i++)
        {
            tooltips[i].SetActive(true);

            float waitTime = 0f;

            if (audioClips.Length > i && audioClips[i] != null)
            {
                audioSource.clip = audioClips[i];
                if (!skipAudio)
                {
                    audioSource.Play();
                    waitTime = audioClips[i].length;
                }
                else
                {
                    waitTime = 1f; 
                }
            }

            yield return new WaitForSeconds(waitTime);

            tooltips[i].SetActive(false);
        }

        Debug.Log("Preparing to open dialog...");
        Dialog myDialog = Dialog.Open(dialogPrefab);

        if (myDialog != null)
        {
            myDialog.OnClosed += result =>
            {
                switch (result.Result)
                {
                    case DialogButtonType.Yes:
                        StartCoroutine(ShowTooltipsSequentially());
                        break;
                    case DialogButtonType.No:
                        //sceneTransition.BeginTransition();
                        //testing hand menu and skip the scene 2
                        MenuIntroController.StartIntro();
                        clickableObj.SetActive(true);
                        if (sceneTransitionObj != null)
                        {
                            sceneTransitionObj.SetActive(true);
                        }
                        if (sceneTransition != null)
                        {
                            sceneTransition.BeginTransition();
                        }
                        if (scene3Timer != null)
                        {
                            scene3Timer.GetComponent<Timer>()?.StartCountdown();
                        }

                        break;
                }
            };
        }
    }
}