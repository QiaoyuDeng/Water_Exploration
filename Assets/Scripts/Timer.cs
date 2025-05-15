using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;

public class Timer : MonoBehaviour
{
    public float explorationTime = 360f;
    private float timer;
    public GameObject dialogPrefab;
    private bool isCounting = false;
    public GameObject handMenu;
    public GameObject clickableObj;

    [Header("Rain Particle Control")]
    public ParticleSystem rainParticleSystem;
    public AudioSource rainAudioSource;


    public MRTKSceneTransition sceneTransition;

    void Update()
    {
        if (!isCounting) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isCounting = false;
            ShowDialogue();

        }
    }

    public void StartCountdown()
    {
        timer = explorationTime;
        isCounting = true;
    }

    public void ShowDialogue()
    {
        Dialog myDialog = Dialog.Open(dialogPrefab);
        if (myDialog != null)
        {
            myDialog.OnClosed += result =>
            {
                switch (result.Result)
                {
                    case DialogButtonType.Yes:
                        sceneTransition.BeginTransition();
                        handMenu.SetActive(false);
                        clickableObj.SetActive(true);

                        if (rainParticleSystem != null)
                        {
                            rainParticleSystem.Stop();
                            rainAudioSource.Stop();
                        }

                        break;
                }
            };

        }
    }
}
