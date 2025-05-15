using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class FarmController : MonoBehaviour
{
    public CSVReader csvReader;

    public FarmSimulator smallFarm;
    public FarmSimulator mediumFarm;
    public FarmSimulator largeFarm;

    [Header("Rain Particle Control")]
    public ParticleSystem rainParticleSystem;

    [Header("Rain Audio Clips")]
    public AudioSource rainAudioSource;
    public AudioClip lightRainAudioBackground;
    public AudioClip moderateRainAudioBackground;
    public AudioClip heavyRainAudioBackground;

    public AudioClip lightRainAudio;
    public AudioClip moderateRainAudio;
    public AudioClip heavyRainAudio;

    [Header("Overlay & Control")]
    public ScenarioOverlayController overlayController;
    public PlayNarrationManager narrationManager;
    public TextMeshProUGUI dayLabel;
    public Interactable previousButton;
    public Interactable nextButton;
    public GameObject dayControlGroup;

    //private string[] rainfallScenarios = { "LightRainfall", "ModerateRainfall", "HeavyRainfall" };
    private string[] rainfallScenarios = { "LightRainfall",  "HeavyRainfall" };
    private int stepsPerScenario = 7;
    private int currentScenarioIndex = 0;
    private int currentDay = -1;
    private bool isAutoPlaying = false;
    private Coroutine playbackCoroutine;

    public bool introFinished = false;

    [Header("Dialog Control")]
    public GameObject dialogPrefabLarge;  
    public MRTKSceneTransition sceneTransition;

    [Header("Hand Menu")]
    public GameObject handMenu;
    public GameObject clickableObj;
    public GameObject sceneTransitionObj;

    [Header("Reuse Fill UI Text")]
    public TextMeshProUGUI smallFarmFillText;
    public TextMeshProUGUI mediumFarmFillText;
    public TextMeshProUGUI largeFarmFillText;
    public GameObject smallFillTextGroup;
    public GameObject mediumFillTextGroup;
    public GameObject largeFillTextGroup;

    [Header("Overflow Percent V.S. Small")]
    public TextMeshProUGUI smallOverflowPercentText;
    public TextMeshProUGUI mediumOverflowPercentText;
    public TextMeshProUGUI largeOverflowPercentText;
    public GameObject smallOverflowPercentGroup;
    public GameObject mediumOverflowPercentGroup;
    public GameObject largeOverflowPercentGroup;

    [Header("Total P Volume")]
    public TextMeshProUGUI smallTotalPText;
    public TextMeshProUGUI mediumTotalPText;
    public TextMeshProUGUI largeTotalPText;
    public GameObject smallTotalPGroup;
    public GameObject mediumTotalPGroup;
    public GameObject largeTotalPGroup;

    [Header("Total P V.S. Small")]
    public TextMeshProUGUI smallTotalPPercentText;
    public TextMeshProUGUI mediumTotalPPercentText;
    public TextMeshProUGUI largeTotalPPercentText;
    public GameObject smallTotalPPercentGroup;
    public GameObject mediumTotalPPercentGroup;
    public GameObject largeTotalPPercentGroup;


    private IEnumerator Start()
    {
        csvReader.ReadCSV();
        if (dayControlGroup != null)
        {
            dayControlGroup.SetActive(false);
        }

        smallFarm.SetPumpBackReuseRate(-5);
        mediumFarm.SetPumpBackReuseRate(-10);
        largeFarm.SetPumpBackReuseRate(-20);

        yield return new WaitUntil(() => introFinished);

        isAutoPlaying = true;

        for (int scenarioIndex = 0; scenarioIndex < rainfallScenarios.Length; scenarioIndex++)
        {
            smallFarm.ResetOverflowCube();
            mediumFarm.ResetOverflowCube();
            largeFarm.ResetOverflowCube();

            smallFillTextGroup.SetActive(true);
            mediumFillTextGroup.SetActive(true);
            largeFillTextGroup.SetActive(true);

            smallOverflowPercentGroup.SetActive(true);
            mediumOverflowPercentGroup.SetActive(true);
            largeOverflowPercentGroup.SetActive(true);

            smallTotalPGroup.SetActive(true);
            mediumTotalPGroup.SetActive(true);
            largeTotalPGroup.SetActive(true);

            smallTotalPPercentGroup.SetActive(true);
            mediumTotalPPercentGroup.SetActive(true);
            largeTotalPPercentGroup.SetActive(true);


            currentScenarioIndex = scenarioIndex;
            string rain = rainfallScenarios[scenarioIndex];

            if (overlayController != null)
            {
                string rainLabel = rain switch
                {
                    "LightRainfall" => "Scenario 1: Light Rainfall",
                    "ModerateRainfall" => "Scenario 2: Moderate Rainfall",
                    "HeavyRainfall" => "Scenario 2: Heavy Rainfall",
                    _ => "Scenario: Unknown"
                };

                AudioClip clip = rain switch
                {
                    "LightRainfall" => lightRainAudio,
                    "ModerateRainfall" => moderateRainAudio,
                    "HeavyRainfall" => heavyRainAudio,
                    _ => null
                };

                Debug.Log($"Showing rain overlay: {rainLabel}, clip: {(clip != null ? clip.name : "null")}");
                yield return StartCoroutine(overlayController.ShowScenarioText(rainLabel, clip));
                Debug.Log("✅ Finished rain overlay");
            }

            Debug.Log("Starting rain visual effect");
            SetRainByScenario(rain);

            if (dayControlGroup != null)
            {
                dayControlGroup.SetActive(true);
            }
            playbackCoroutine = StartCoroutine(PlayFromDay(0));
            yield return playbackCoroutine;
        }



        isAutoPlaying = false;
    }

    public void GoToPreviousDay()
    {
        if (currentDay > 0)
        {
            currentDay--;
            StopAllPlayback();
            playbackCoroutine = StartCoroutine(PlayFromDay(currentDay));
        }
    }

    public void GoToNextDay()
    {
        if (currentDay < stepsPerScenario - 1)
        {
            currentDay++;
            StopAllPlayback();
            playbackCoroutine = StartCoroutine(PlayFromDay(currentDay));
        }
    }

    private void StopAllPlayback()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }

        narrationManager.StopNarration();
        smallFarm.StopAllAnimations();
        mediumFarm.StopAllAnimations();
        largeFarm.StopAllAnimations();
    }

    public IEnumerator PlayFromDay(int startDay)
    {
        string rain = rainfallScenarios[currentScenarioIndex];


        SetRainByScenario(rain);

        // adjust heightscale to avoid high values
        float heightScale = (rain == "HeavyRainfall") ? 0.01f : 1f;
        smallFarm.heightScale = heightScale;
        mediumFarm.heightScale = heightScale;
        largeFarm.heightScale = heightScale;

        List<float> smallOverflow = csvReader.GetColumnValues($"5ML_{rain}_OverflowPlux");
        List<float> mediumOverflow = csvReader.GetColumnValues($"10ML_{rain}_OverflowPlux");
        List<float> largeOverflow = csvReader.GetColumnValues($"20ML_{rain}_OverflowPlux");

        List<float> smallOverflowPercent = csvReader.GetColumnValues($"5ML_{rain}_OverflowPluxPercentSmall");
        List<float> mediumOverflowPercent = csvReader.GetColumnValues($"10ML_{rain}_OverflowPluxPercentSmall");
        List<float> largeOverflowPercent = csvReader.GetColumnValues($"20ML_{rain}_OverflowPluxPercentSmall");

        List<float> smallVolume = csvReader.GetColumnValues($"5ML_{rain}_StorageVolume");
        List<float> mediumVolume = csvReader.GetColumnValues($"10ML_{rain}_StorageVolume");
        List<float> largeVolume = csvReader.GetColumnValues($"20ML_{rain}_StorageVolume");

        List<float> smallTP = csvReader.GetColumnValues($"5ML_{rain}_TPFarm");
        List<float> mediumTP = csvReader.GetColumnValues($"10ML_{rain}_TPFarm");
        List<float> largeTP = csvReader.GetColumnValues($"20ML_{rain}_TPFarm");

        List<float> smallFillList = csvReader.GetColumnValues($"5ML_{rain}_ReuseSystemFill");
        List<float> mediumFillList = csvReader.GetColumnValues($"10ML_{rain}_ReuseSystemFill");
        List<float> largeFillList = csvReader.GetColumnValues($"20ML_{rain}_ReuseSystemFill");

        List<float> smallTotalP = csvReader.GetColumnValues($"5ML_{rain}_PfluxFarm_acc");
        List<float> mediumTotalP = csvReader.GetColumnValues($"10ML_{rain}_PfluxFarm_acc");
        List<float> largeTotalP = csvReader.GetColumnValues($"20ML_{rain}_PfluxFarm_acc");

        List<float> smallTotalPPercent = csvReader.GetColumnValues($"5ML_{rain}_PfluxFarmPercentSmall");
        List<float> mediumTotalPPercent = csvReader.GetColumnValues($"10ML_{rain}_PfluxFarmPercentSmall");
        List<float> largeTotalPPercent = csvReader.GetColumnValues($"20ML_{rain}_PfluxFarmPercentSmall");



        for (int day = startDay; day < stepsPerScenario; day++)
        {
            currentDay = day;
            UpdateDayLabel(day);
            UpdateDayButtonStates();

            smallFarm.paddockTPValue = smallTP[day];
            mediumFarm.paddockTPValue = mediumTP[day];
            largeFarm.paddockTPValue = largeTP[day];


            smallFarmFillText.text = $"{smallFillList[day]}%";
            mediumFarmFillText.text = $"{mediumFillList[day]}%";
            largeFarmFillText.text = $"{largeFillList[day]}%";

            smallOverflowPercentText.text = $"{smallOverflowPercent[day]}%";
            mediumOverflowPercentText.text = $"{mediumOverflowPercent[day]}%";
            largeOverflowPercentText.text = $"{largeOverflowPercent[day]}%";

            // Total Phosphorus Accumulated
            smallTotalPText.text = $"{smallTotalP[day]}kg";
            mediumTotalPText.text = $"{mediumTotalP[day]}kg";
            largeTotalPText.text = $"{largeTotalP[day]}kg";

            // Total Phosphorus Percent vs Small
            smallTotalPPercentText.text = $"{smallTotalPPercent[day]}%";
            mediumTotalPPercentText.text = $"{mediumTotalPPercent[day]}%";
            largeTotalPPercentText.text = $"{largeTotalPPercent[day]}%";

            yield return StartCoroutine(PlayDay(day, rain, smallVolume, mediumVolume, largeVolume, smallOverflow, mediumOverflow, largeOverflow));
        }

        // Only trigger transition if it's the last day
        if (currentScenarioIndex < rainfallScenarios.Length - 1 && currentDay >= stepsPerScenario - 1)
        {
            currentScenarioIndex++;
            currentDay = 0;

            string nextRain = rainfallScenarios[currentScenarioIndex];
            string rainLabel = nextRain switch
            {
                "LightRainfall" => "Scenario 1: Light Rainfall",
                "ModerateRainfall" => "Scenario 2: Moderate Rainfall",
                "HeavyRainfall" => "Scenario 2: Heavy Rainfall",
                _ => "Scenario: Unknown"
            };
            AudioClip clip = nextRain switch
            {
                "LightRainfall" => lightRainAudio,
                "ModerateRainfall" => moderateRainAudio,
                "HeavyRainfall" => heavyRainAudio,
                _ => null
            };

            if (overlayController != null)
            {
                yield return StartCoroutine(overlayController.ShowScenarioText(rainLabel, clip));
            }

            smallFarm.ResetOverflowCube();
            mediumFarm.ResetOverflowCube();
            largeFarm.ResetOverflowCube();

            playbackCoroutine = StartCoroutine(PlayScenarioFromCurrentDay());
        }

        if (currentScenarioIndex == rainfallScenarios.Length - 1 && currentDay == stepsPerScenario - 1)
        {
            Debug.Log("🎬 All scenarios complete. Showing next stage dialogue!");

            Dialog myDialog = Dialog.Open(dialogPrefabLarge);

            if (myDialog != null)
            {
                myDialog.OnClosed += result =>
                {
                    switch (result.Result)
                    {
                        case DialogButtonType.Yes:
                            Debug.Log("✅ User clicked YES - Starting transition to exploration view!");
                            
                            Application.Quit();
                            // if (rainParticleSystem != null)
                            // {
                            //     rainParticleSystem.Stop();
                            //     rainAudioSource.Stop();
                            // }

                            // smallFarm.HideOverflowObjects();
                            // mediumFarm.HideOverflowObjects();
                            // largeFarm.HideOverflowObjects();

                            // if (dayControlGroup != null)
                            // {
                            //     dayControlGroup.SetActive(false);
                            // }


                            // if (sceneTransitionObj != null)
                            // {
                            //     sceneTransitionObj.SetActive(true);
                            // }

                            // if (sceneTransition != null)
                            // {
                            //     sceneTransition.BeginTransition();
                            // }


                            // if (handMenu != null)
                            // {
                            //     handMenu.SetActive(true);
                            // }

                            // if (clickableObj != null)
                            // {
                            //     clickableObj.SetActive(true);
                            // }
                            


                            break;
                    }
                };
            }
        }
    }

    private IEnumerator PlayScenarioFromCurrentDay()
    {
        yield return PlayFromDay(currentDay);
    }

    private IEnumerator PlayDay(int day, string rain, List<float> smallVolume, List<float> mediumVolume, List<float> largeVolume, List<float> smallOverflow, List<float> mediumOverflow, List<float> largeOverflow)
    {
        yield return new WaitUntil(() =>
            smallFarm.isReadyForNext &&
            mediumFarm.isReadyForNext &&
            largeFarm.isReadyForNext);

        float cumulativeSmallOverflow = 0f;
        float cumulativeMediumOverflow = 0f;
        float cumulativeLargeOverflow = 0f;

        for (int i = 0; i <= day; i++)
        {
            cumulativeSmallOverflow += smallOverflow[i];
            cumulativeMediumOverflow += mediumOverflow[i];
            cumulativeLargeOverflow += largeOverflow[i];
        }

        smallFarm.SetReuseValues(smallVolume[day], smallOverflow[day], cumulativeSmallOverflow);
        mediumFarm.SetReuseValues(mediumVolume[day], mediumOverflow[day], cumulativeMediumOverflow);
        largeFarm.SetReuseValues(largeVolume[day], largeOverflow[day], cumulativeLargeOverflow);


        smallFarm.currentDay = day;
        mediumFarm.currentDay = day;
        largeFarm.currentDay = day;

        Coroutine narrationCoroutine = StartCoroutine(narrationManager.PlayNarrationAndWait(currentScenarioIndex, day, 0));
        Coroutine smallAnim = StartCoroutine(smallFarm.AnimateScenarioSilent());
        Coroutine mediumAnim = StartCoroutine(mediumFarm.AnimateScenarioSilent());
        Coroutine largeAnim = StartCoroutine(largeFarm.AnimateScenarioSilent());

        yield return new WaitUntil(() =>
            smallFarm.isReadyForNext &&
            mediumFarm.isReadyForNext &&
            largeFarm.isReadyForNext &&
            !narrationManager.audioSource.isPlaying);

        if (day == stepsPerScenario - 1)
        {
            Debug.Log($"✅ Triggering final narration for scenario {currentScenarioIndex}, day {day}, step 99");
            yield return StartCoroutine(narrationManager.PlayNarrationAndWait(currentScenarioIndex, day, 99));
        }
        else
        {
            Debug.Log($"🔸 Not final day: day = {day}");
        }

    }

    public void PlayOnlyCurrentDay(int dayIndex, string rain)
    {
        Debug.Log($"🎬 Playing only Day {dayIndex + 1} from HandMenu Slider with Rainfall={rain}");

        StopAllPlayback(); 

        currentDay = dayIndex;
        smallFarm.currentDay = dayIndex;
        mediumFarm.currentDay = dayIndex;
        largeFarm.currentDay = dayIndex;

        if (smallFarm != null) StartCoroutine(smallFarm.AnimateScenarioSilent());
        if (mediumFarm != null) StartCoroutine(mediumFarm.AnimateScenarioSilent());
        if (largeFarm != null) StartCoroutine(largeFarm.AnimateScenarioSilent());
    }






    private void UpdateDayLabel(int day)
    {
        if (dayLabel != null)
        {
            dayLabel.text = $"Day {day + 1}";
        }
    }

    private void UpdateDayButtonStates()
    {
        if (previousButton != null)
        {
            previousButton.IsEnabled = currentDay > 0;
        }
        if (nextButton != null)
        {
            nextButton.IsEnabled = currentDay < stepsPerScenario - 1;
        }
    }

    public void MarkIntroAsFinished()
    {
        introFinished = true;
    }

    public void SetRainByScenario(string scenario)
    {
        if (rainParticleSystem == null)
        {
            Debug.LogWarning("❗ Rain particle system not assigned.");
            return;
        }

        var emission = rainParticleSystem.emission;
        var main = rainParticleSystem.main;

        if (scenario == "LightRainfall")
        {
            emission.rateOverTime = 10;
            main.startSize = 0.05f;
            rainAudioSource.clip = lightRainAudioBackground;
            Debug.Log("Light rainfall started.");
        }
        else if (scenario == "ModerateRainfall")
        {
            emission.rateOverTime = 100;
            main.startSize = 0.08f;
            rainAudioSource.clip = moderateRainAudioBackground;
            Debug.Log("Moderate rainfall started.");
        }
        else if (scenario == "HeavyRainfall")
        {
            emission.rateOverTime = 200;
            main.startSize = 0.1f;
            rainAudioSource.clip = heavyRainAudioBackground;
            Debug.Log("Heavy rainfall started.");
        }

        rainAudioSource.Play();
        rainParticleSystem.Play();
    }

}
