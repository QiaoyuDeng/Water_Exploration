using UnityEngine;
using ChartAndGraph;
using System.Linq;

public class AnalysisController : MonoBehaviour
{
    [Header("Dashboard Panels")]
    public GameObject analysisDashboard;

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

    private string lastFarmSize = "";
    private string lastScenario = "";

    private void Start()
    {

        farmBarChart = farmBarChartObject.GetComponent<BarChart>();
        outflowBarChart = outflowBarChartObject.GetComponent<BarChart>();
    }

    public void StartAnalysis()
    {
        analysisDashboard.SetActive(true);

        UpdateAnalysisCharts();
    }

    public void ReturnToIntro()
    {
        analysisDashboard.SetActive(false);
    }

    private void UpdateAnalysisCharts()
    {
        if (csvReader == null || farmBarChart == null || outflowBarChart == null)
        {
            Debug.LogWarning("❗ Missing references!");
            return;
        }

        string currentFarmSize = menuIntroController.GetCurrentFarmSize();
        string currentScenario = menuIntroController.GetCurrentScenario();

        string farmFluxCol = $"{currentFarmSize}_{currentScenario}_PfluxFarm_acc";
        string outflowFluxCol = $"{currentFarmSize}_{currentScenario}_OverflowPlux_acc";

        var farmFluxValues = csvReader.GetColumnValues(farmFluxCol);
        var outflowFluxValues = csvReader.GetColumnValues(outflowFluxCol);

        int days = Mathf.Min(farmFluxValues.Count, outflowFluxValues.Count, 7);


        // set the automatic axis of the bar chart data
        if (farmFluxValues.All(v => v == 0))
        {
            farmBarChart.DataSource.AutomaticMaxValue = false;
            farmBarChart.DataSource.MaxValue = 5f;
        }
        else 
        {
            farmBarChart.DataSource.AutomaticMaxValue = true;
        }

        if (outflowFluxValues.All(v => v == 0))
        {
            outflowBarChart.DataSource.AutomaticMaxValue = false;
            outflowBarChart.DataSource.MaxValue = 5f;
        }
        else
        {
            outflowBarChart.DataSource.AutomaticMaxValue = true;
        }


        for (int i = 0; i < days; i++)
        {
            string groupName = $"Day {i + 1}";
            farmBarChart.DataSource.SetValue(groupName, "All", farmFluxValues[i]);
            outflowBarChart.DataSource.SetValue(groupName, "All", outflowFluxValues[i]);
        }

        if (titleText != null) titleText.text = "Analysis";
        if (leftChartTitle != null) leftChartTitle.text = "Accumulated Farm P Volume";
        if (rightChartTitle != null) rightChartTitle.text = "Accumulated Outflow P Volume";

        if (descriptionText != null)
        {
            string description = GenerateDescription(currentFarmSize, currentScenario);
            descriptionText.text = description;
        }


        Debug.Log("✅ Analysis charts updated using pre-accumulated values.");
    }

    private string GenerateDescription(string farmSize, string scenario)
    {
        switch (farmSize)
        {
            case "5ML":
                switch (scenario)
                {
                    case "LightRainfall":
                        return "A farm with a small reuse system under light rainfall. Phosphorus accumulation remains low and stable over the week, with little overflow detected.";
                    case "ModerateRainfall":
                        return "A farm with a small reuse system facing moderate rainfall. Phosphorus flux increases slightly but remains moderate, and overflow is increased.";
                    case "HeavyRainfall":
                        return "A farm with a small reuse system experiencing heavy rainfall. Rapid phosphorus accumulation and significant overflow events observed starting from Day 3.";
                }
                break;
            case "10ML":
                switch (scenario)
                {
                    case "LightRainfall":
                        return "A farm with a medium reuse system under light rainfall. Phosphorus levels remain very low and stable, with no overflow occurring.";
                    case "ModerateRainfall":
                        return "A farm with a medium reuse system during moderate rainfall. Moderate increases in phosphorus accumulation with no overflow.";
                    case "HeavyRainfall":
                        return "A farm with a medium reuse system facing heavy rainfall. Rapid phosphorus buildup accompanied by noticeable overflow beginning around Day 3.";
                }
                break;
            case "20ML":
                switch (scenario)
                {
                    case "LightRainfall":
                        return "A farm with a large reuse system under light rainfall. Very stable phosphorus levels and no overflow detected throughout the week.";
                    case "ModerateRainfall":
                        return "A farm with a large reuse system during moderate rainfall. Steady phosphorus accumulation with no overflow.";
                    case "HeavyRainfall":
                        return "A farm with a large reuse system facing heavy rainfall. Significant phosphorus accumulation and substantial overflow detected after Day 3.";
                }
                break;
        }

        return "No description available for the selected condition."; // fallback if unexpected input
    }

    private void Update()
    {
        if (!analysisDashboard.activeSelf)
            return; // If analysis panel is not open, do nothing

        string currentFarmSize = menuIntroController.GetCurrentFarmSize();
        string currentScenario = menuIntroController.GetCurrentScenario();

        if (currentFarmSize != lastFarmSize || currentScenario != lastScenario)
        {
            Debug.Log($"🔄 Detected change: {lastFarmSize}→{currentFarmSize} or {lastScenario}→{currentScenario}");

            lastFarmSize = currentFarmSize;
            lastScenario = currentScenario;

            UpdateAnalysisCharts(); // Auto refresh charts
        }
    }


}
