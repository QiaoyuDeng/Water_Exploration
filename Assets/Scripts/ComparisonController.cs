using UnityEngine;
using ChartAndGraph;

public class ComparisonController : MonoBehaviour
{
    [Header("Dashboard Panels")]
    public GameObject comparisonDashboard;

    [Header("Bar Charts")]
    public GameObject farmBarChartObject;
    public GameObject outflowBarChartObject;
    private BarChart farmBarChart;
    private BarChart outflowBarChart;

    [Header("Controllers")]
    public CSVReader csvReader;
    public MenuIntroController menuIntroController;

    [Header("Texts")]
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text leftChartTitle;
    public TMPro.TMP_Text rightChartTitle;
    public TMPro.TMP_Text descriptionText;

    private string lastScenario = "";
    private bool hasStartedComparison = false;

    private void Start()
    {
       farmBarChart = farmBarChartObject.GetComponent<BarChart>();
        outflowBarChart = outflowBarChartObject.GetComponent<BarChart>();
    }

    public void StartComparison()
    {
        comparisonDashboard.SetActive(true);

        UpdateComparisonCharts();
    }

    public void ReturnToIntro()
    {
        comparisonDashboard.SetActive(false);
    }

    private void UpdateComparisonCharts()
    {
        if (csvReader == null || farmBarChart == null || outflowBarChart == null)
        {
            Debug.LogWarning("Missing references!");
            return;
        }

        string currentScenario = menuIntroController.GetCurrentScenario();
        Debug.Log($"Current Scenario Selected: {currentScenario}");

        string[] farmSizes = { "5ML", "10ML", "20ML" };
        string[] displayNames = { "Small", "Medium", "Large" };

        // set the automatic axis of the bar chart data
        farmBarChart.DataSource.AutomaticMaxValue = true;
        outflowBarChart.DataSource.AutomaticMaxValue = true;

        for (int i = 0; i < farmSizes.Length; i++)
        {
            string farmSize = farmSizes[i];
            string displayName = displayNames[i];

            string farmFluxCol = $"{farmSize}_{currentScenario}_PfluxFarm_acc";
            string overflowFluxCol = $"{farmSize}_{currentScenario}_OverflowPlux_acc";

            Debug.Log($"🔍 Fetching columns: {farmFluxCol} and {overflowFluxCol}");

            var farmFluxValues = csvReader.GetColumnValues(farmFluxCol);
            var overflowFluxValues = csvReader.GetColumnValues(overflowFluxCol);

            float farmTotal = farmFluxValues[6];  
            float overflowTotal = overflowFluxValues[6];

            farmBarChart.DataSource.SetValue(displayName, "All", farmTotal);
            outflowBarChart.DataSource.SetValue(displayName, "All", overflowTotal);

            Debug.Log($"🎯 Set chart value for {displayName}");
        }

        if (titleText != null) titleText.text = "Comparison";
        if (leftChartTitle != null) leftChartTitle.text = "Farm P Accumulation Across Reuse Systems";
        if (rightChartTitle != null) rightChartTitle.text = "Overflow P Accumulation Across Reuse Systems";

        if (descriptionText != null)
        {
            string description = GenerateComparisonDescription(currentScenario);
            descriptionText.text = description;
        }
    }
    private string GenerateComparisonDescription(string scenario)
    {
        switch (scenario)
        {
            case "LightRainfall":
                return "Under light rainfall conditions, only the small farm shows low phosphorus accumulation with negligible overflow. Reuse systems effectively retain phosphorus.";
            case "ModerateRainfall":
                return "Moderate rainfall leads to increased phosphorus accumulation, with minor overflow differences among reuse systems.";
            case "HeavyRainfall":
                return "Heavy rainfall causes significant phosphorus runoff. Farms with larger reuse systems manage overflow better compared to small systems.";
        }

        return "Comparison data unavailable for the selected condition."; // fallback
    }

    private void Update()
    {
        if (!comparisonDashboard.activeSelf)
            return; 

        string currentScenario = menuIntroController.GetCurrentScenario();

        if (!hasStartedComparison)
        {
            
            lastScenario = currentScenario;
            hasStartedComparison = true;
            Debug.Log($"Initial load: {lastScenario}");
            UpdateComparisonCharts();
            return;
        }

        if (currentScenario != lastScenario)
        {
            Debug.Log($"Detected scenario change: {lastScenario} -> {currentScenario}");
            lastScenario = currentScenario;
            UpdateComparisonCharts();
        }
    }


}

