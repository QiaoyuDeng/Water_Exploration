using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;

public class FarmIntroSequence : MonoBehaviour
{
    [Header("Floor Objects")]
    public GameObject[] floorObjects;             // Farm objects
    public Material highlightMaterial;            // Highlight material
    public AudioClip[] introAudioClips;           // Audio clips for the introduction (4 parts)
    public AudioSource audioSource;               // Audio source to play the clips
    public GameObject dialogPrefab;
    public FarmController controller;            // Reference to the FarmController to play animations

    public float displayTime = 3f;                // Duration for each highligh
    private Material[] originalMaterials;         // Store original materials

    [Header("Testing Options")]
    public bool skipAudio = false; // If true, skip audio wait and use 1s per clip 

    [Header("Farm PFlux Objects")]
    public GameObject smallFarmPFlux;
    public GameObject mediumFarmPFlux;
    public GameObject largeFarmPFlux;

    void Start()
    {
        // Store the original material of each floor object
        originalMaterials = new Material[floorObjects.Length];
        for (int i = 0; i < floorObjects.Length; i++)
        {
            Renderer renderer = floorObjects[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials[i] = renderer.material;
            }
        }
    }

    public void StartFarmIntro()
    {
        StartCoroutine(HighlightFarmsSequentially());
    }

    // Sequentially highlight each farm and play the corresponding audio
    private IEnumerator HighlightFarmsSequentially()
    {
        // First audio: Introduction to the scene
        if (introAudioClips.Length > 0 && introAudioClips[0] != null)
        {
            audioSource.clip = introAudioClips[0];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);  // Wait for audio to finish
        }

        for (int i = 0; i < floorObjects.Length; i++)
        {
            // Highlight the current floor object
            Renderer renderer = floorObjects[i].GetComponent<Renderer>();
            if (renderer != null && highlightMaterial != null)
            {
                renderer.material = highlightMaterial;
            }

            // Play the corresponding audio clip for the current farm
            if (introAudioClips.Length > i + 1 && introAudioClips[i + 1] != null)
            {
                audioSource.clip = introAudioClips[i + 1];
                audioSource.Play();
                // yield return new WaitForSeconds(audioSource.clip.length);  // Wait for audio to finish
                yield return new WaitForSeconds(skipAudio ? 1f : audioSource.clip.length);

            }
            else
            {
                yield return new WaitForSeconds(displayTime);  // Default wait time if no audio
            }

            // Restore the original material of the current floor object
            if (renderer != null && originalMaterials[i] != null)
            {
                renderer.material = originalMaterials[i];
            }
        }

        yield return StartCoroutine(IntroduceFarmIndicators());

        Dialog myDialog = Dialog.Open(
            dialogPrefab
        );

        if (myDialog != null)
        {
            myDialog.OnClosed += result =>
            {
                switch (result.Result)
                {
                    case DialogButtonType.Yes:
                        StartCoroutine(HighlightFarmsSequentially()); // Replay
                        break;
                    case DialogButtonType.No:
                        Debug.Log("Start Simulation");
                        //start simulation
                        controller.MarkIntroAsFinished();
                        break;
                }
            };
        }


        Debug.Log("Farm highlight sequence finished.");
    }

    private IEnumerator IntroduceFarmIndicators()
    {
        Debug.Log("Highlighting key phosphorus indicators...");

        
        GameObject[][] indicatorGroups = new GameObject[][]
        {
        new GameObject[] {
            smallFarmPFlux,
            mediumFarmPFlux,
            largeFarmPFlux
        },
        new GameObject[] {
            controller.smallFarm.OverflowPercentGroup,
            controller.mediumFarm.OverflowPercentGroup,
            controller.largeFarm.OverflowPercentGroup
        },
        new GameObject[] {
            controller.smallFarm.ReuseFillTextGroup,
            controller.mediumFarm.ReuseFillTextGroup,
            controller.largeFarm.ReuseFillTextGroup
        },
        new GameObject[] {
            controller.smallFarm.TotalPGroup,
            controller.mediumFarm.TotalPGroup,
            controller.largeFarm.TotalPGroup
        },
        new GameObject[] {
            controller.smallFarm.TotalPPercentGroup,
            controller.mediumFarm.TotalPPercentGroup,
            controller.largeFarm.TotalPPercentGroup
        }

        };

        int audioOffset = floorObjects.Length + 1; 

        for (int groupIndex = 0; groupIndex < indicatorGroups.Length; groupIndex++)
        {
            GameObject[] group = indicatorGroups[groupIndex];
            Material[] originalMats = new Material[group.Length];

            
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i] != null)
                    group[i].SetActive(true);
            }

            yield return null; 

            
            for (int i = 0; i < group.Length; i++)
            {
                Renderer r = group[i].GetComponent<Renderer>();
                if (r != null)
                {
                    originalMats[i] = r.material;
                    r.material = highlightMaterial;
                }
            }

            
            int audioIndex = audioOffset + groupIndex;
            if (introAudioClips.Length > audioIndex && introAudioClips[audioIndex] != null)
            {
                audioSource.clip = introAudioClips[audioIndex];
                audioSource.Play();
                yield return new WaitForSeconds(skipAudio ? 1f : audioSource.clip.length);
            }
            else
            {
                yield return new WaitForSeconds(displayTime);
            }

            
            for (int i = 0; i < group.Length; i++)
            {
                Renderer r = group[i].GetComponent<Renderer>();
                if (r != null && originalMats[i] != null)
                {
                    r.material = originalMats[i];
                }

                group[i].SetActive(false);
            }
        }

        Debug.Log("Finished indicator highlighting.");
    }


}
