using UnityEngine;
using System.Collections;
using ChartAndGraph;


public class MenuIntroController : MonoBehaviour
{
    public GameObject nearMenu;

    [Header("Dropdown Menus")]
    public GameObject farmsizeDropMenu;
    public GameObject scenaryDropMenu;
    public GameObject periodDropMenu;
    public CSVReader csvReader;

    // Put all menus into an array for batch processing
    private GameObject[] allDropdowns;
    private string currentFarmSize = "5ML";
    private string currentScenario = "LightRainfall";
    private string currentTargetName = "IrrigationChannel";

    public DataDisplay idatadisplay;

    // Control the dashboard display
    public GameObject dataBoard;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text descriptionText;
    public TMPro.TMP_Text unitText;
    public GameObject barChartObject;  // Drag your chart into the Inspector
    private BarChart barChart;         // Actual components in use

    [Header("Rain Control")]
    public ParticleSystem rainParticleSystem;

    // Field to save previous highlighted controller
    public Material highlightMaterial;
    private ChartMaterialController lastHighlightedController = null;

    // Animation
    public FarmController farmController;

    [Header("Teleport Targets")]
    public Transform smallFarmTeleportTarget;
    public Transform mediumFarmTeleportTarget;
    public Transform largeFarmTeleportTarget;
    public MRTKSceneTransition sceneTransition;

    [Header("Rain Audio Clips")]
    public AudioSource rainAudioSource;
    public AudioClip lightRainAudioBackground;
    public AudioClip moderateRainAudioBackground;
    public AudioClip heavyRainAudioBackground;

    [Header("Hand Menu Labels")]
    public TMPro.TextMeshPro reuseSystemLabel;
    public TMPro.TextMeshPro rainfallLabel;

    [Header("Task Board")]
    public GameObject taskBoard;

    void Start()
    {
        // Initialize the dropdown menu list
        allDropdowns = new GameObject[] { farmsizeDropMenu, scenaryDropMenu, periodDropMenu };
        csvReader.ReadCSV();
    }

    public void StartIntro()
    {
        nearMenu.SetActive(true);

        //StartCoroutine(PlayMenuIntro());
    }

    // Triggered when the button is clicked: Show the corresponding menu, hide others
    public void ToggleDropdown(GameObject targetDropdown)
    {
        foreach (GameObject dropdown in allDropdowns)
        {
            if (dropdown != null)
            {
                // Only show the target menu, hide others
                dropdown.SetActive(dropdown == targetDropdown && !dropdown.activeSelf);
            }
        }
    }
    public void OnFarmSizeSelected(string size)
    {
        currentFarmSize = size;
        UpdateDataColumn();

        if (reuseSystemLabel != null)
            reuseSystemLabel.text = size;
    }

    public void OnScenarioSelected(string scenario)
    {
        currentScenario = scenario;
        UpdateDataColumn();

        SetRainByScenario(scenario);

        if (rainfallLabel != null)
            rainfallLabel.text = scenario;
    }

    public void OnTargetSelected(string target)
    {
        currentTargetName = target;
        UpdateDataColumn(); // renew the data column
    }

    private void UpdateDataColumn()
    {
        string col = $"{currentFarmSize}_{currentScenario}_{currentTargetName}"; 
        idatadisplay.columnName = col;
        idatadisplay.RebindSliderEvent(); 
        idatadisplay.RefreshDisplay();
        idatadisplay.SetBarChart(barChartObject.GetComponent<BarChart>());
        idatadisplay.UpdateBarChartForWeek();
        Debug.Log("Current column name updated to: " + col);
    }

    public string GetCurrentFarmSize()
    {
        return currentFarmSize;
    }

    public string GetCurrentScenario()
    {
        return currentScenario;
    }

    public string GetCurrentTarget()
    {
        return currentTargetName;
    }

    // This only updates the fixed content (chart) on the dashboard, not the data.
    public void ShowDashboard(string farmSize, string scenario, string targetName, string title, string description, string unit)
    {

        // Set the current column name combination
        currentFarmSize = farmSize;
        currentScenario = scenario;
        currentTargetName = targetName;

        barChart = barChartObject.GetComponent<BarChart>();

        // Update data columns
        UpdateDataColumn();

        // Update title/description
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (unitText != null) unitText.text = unit;

        Debug.Log("Show Dashboard: " + title);
        // Panel
        if (dataBoard != null) dataBoard.SetActive(true);
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

    public void HighlightDayNormal(int dayIndex=4)
    {
        Debug.Log($"[Highlight Debug] Trying to highlight {dayIndex}");

        if (barChart == null)
        {
            if (barChartObject != null)
            {
                barChart = barChartObject.GetComponent<BarChart>();
                Debug.Log($"[Highlight Debug] barChart assigned from barChartObject: {barChartObject.name}");
            }
            else
            {
                Debug.LogError("[Highlight Debug] No barChart assigned!");
                return;
            }
        }

        // Step 1: Restore previous highlighted bar
        if (lastHighlightedController != null)
        {
            Debug.Log($"[Highlight Debug] Restoring previous highlight: {lastHighlightedController.name}");
            lastHighlightedController.OnMouseExit();  
            lastHighlightedController = null;
        }

        // Step 2: Find new bar to highlight
        // Get all controllers
        ChartMaterialController[] controllers = barChart.GetComponentsInChildren<ChartMaterialController>();
        Debug.Log($"[Highlight Debug] Found {controllers.Length} controllers.");

        if (controllers != null && dayIndex >= 0 && dayIndex < controllers.Length)
        {
            ChartMaterialController ctrl = controllers[dayIndex];

            if (ctrl != null)
            {
                Debug.Log($"[Highlight Debug] Found controller for Day {dayIndex + 1}");

                ctrl.OnMouseEnter();  // Simulate hover
                lastHighlightedController = ctrl;
            }
            else
            {
                Debug.LogWarning($"[Highlight Debug] Controller at index {dayIndex} is null!");
            }
        }
        else
        {
            Debug.LogWarning($"[Highlight Debug] Invalid dayIndex: {dayIndex} (controllers length: {controllers.Length})");
        }

    }

    public void TeleportToSelectedFarm()
    {
        string farmSize = GetCurrentFarmSize(); 

        Transform target = null;

        if (farmSize == "5ML")
            target = smallFarmTeleportTarget;
        else if (farmSize == "10ML")
            target = mediumFarmTeleportTarget;
        else if (farmSize == "20ML")
            target = largeFarmTeleportTarget;

        if (target != null && sceneTransition != null)
        {
            sceneTransition.targetView = target; 
            sceneTransition.BeginTransition();   
            Debug.Log($"Teleporting to {farmSize} at {target.position}");
        }
        else
        {
            Debug.LogWarning("Teleport target or sceneTransition not assigned properly!");
        }
    }

    public void OpenTask()
    {
        taskBoard.SetActive(true);
    }





    //private IEnumerator PlayMenuIntro()
    //{
    //    foreach (GameObject go in tooltips)
    //    {
    //        go.SetActive(false);
    //    }

    //    for (int i = 0; i < tooltips.Length; i++)
    //    {
    //        tooltips[i].SetActive(true);

    //        if (i < audioClips.Length && audioClips[i] != null)
    //        {
    //            audioSource.clip = audioClips[i];
    //            audioSource.Play();
    //            yield return new WaitForSeconds(audioSource.clip.length + 0.5f);
    //        }

    //        tooltips[i].SetActive(false);
    //    }

    //    Debug.Log("✅ Near Menu Intro Completed！");
    //}
}
