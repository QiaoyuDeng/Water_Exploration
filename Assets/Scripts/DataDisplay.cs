using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using ChartAndGraph; // Added at the top
using System.Collections.Generic; // Add this line

public class DataDisplay : MonoBehaviour
{
    public CSVReader csvReader;
    public string columnName = "5ML_LightRainfall_IrrigationChannel";
    public PinchSlider pinchSlider; // MRTK Slider
    public TextMeshProUGUI valueDisplayText;
    //public GameObject barChartObject;  // Drag your chart object into the Inspector
    private BarChart barChart;         // The actual BarChart component used

    public MenuIntroController menuIntroController;


    void Start()
    {
        //Debug.Log($"🧠 DataDisplay Start() on {gameObject.name}");

        //if (valueDisplayText == null)
        //    Debug.LogError($"❌ {gameObject.name} valueDisplayText is null!");
        //else
        //    Debug.Log($"✅ {gameObject.name} correctly bound to ValueText: {valueDisplayText.name}");

        //if (barChartObject == null)
        //{
        //    Debug.LogError("❌ barChartObject has not been assigned!");
        //}

        //barChart = barChartObject.GetComponent<BarChart>();

        //if (barChart == null)
        //{
        //    Debug.LogError("❌ Unable to get BarChart component from barChartObject!");
        //}
        //else
        //{
        //    Debug.Log("✅ Successfully acquired BarChart component!");
        //}
        Debug.Log("CSV Loaded: " + csvReader.rowData.Count + " rows");

        //// Listen to slider value changes
        //pinchSlider.OnValueUpdated.AddListener((SliderEventData data) =>
        //{
        //    int dayIndex = Mathf.FloorToInt(data.NewValue * 6); // 0-1 mapped to 0-6
        //    string value = csvReader.GetValue(dayIndex, columnName);
        //    // Just print, no UI text update
        //    Debug.Log($"Day {dayIndex + 1} {columnName} = {value}");

        //    // Update the displayed text
        //    if (valueDisplayText != null)
        //    {
        //        valueDisplayText.text = $"{value}";
        //    }

        //});
        //// Also refresh once at start
        //RefreshDisplay();
    }

    public void RebindSliderEvent()
    {
        Debug.Log($"Calling RebindSliderEvent, valueDisplayText = {valueDisplayText}");
        Debug.Log($"Calling RefreshDisplay() from object: {gameObject.name}, text component: {valueDisplayText}");
        var textRef = valueDisplayText;

        // Remove all previous listeners
        pinchSlider.OnValueUpdated.RemoveAllListeners();

        // Rebind a new listener with the latest columnName
        pinchSlider.OnValueUpdated.AddListener((SliderEventData data) =>
        {
            int dayIndex = Mathf.FloorToInt(data.NewValue * 6);
            string value = csvReader.GetValue(dayIndex, columnName);
            Debug.Log($"[Slider Update] Day {dayIndex + 1} {columnName} = {value}");

            if (textRef != null)
                textRef.text = $"{value}";

            // Highlight the corresponding bar
            if (menuIntroController != null)
            {
                string groupName = $"Day {dayIndex + 1}";
                menuIntroController.HighlightDayNormal(dayIndex);
                Debug.Log($"[Highlight Debug] HighlightDay({groupName}) has been called successfully!");

                string selectedRain = menuIntroController.GetCurrentScenario();
                menuIntroController.farmController.PlayOnlyCurrentDay(dayIndex, selectedRain);

            }
        });

        Debug.Log("Rebound the slider listener!");
    }

    public void SetBarChart(BarChart chart)
    {
        barChart = chart;
        Debug.Log("BarChart assigned by MenuIntroController: " + barChart);
    }

    // ✅ Add this method for external refresh
    public void RefreshDisplay()
    {
        if (csvReader.rowData.Count == 0 || string.IsNullOrEmpty(columnName))
        {
            Debug.LogWarning("Cannot refresh: data not loaded or column name is empty");
            return;
        }

        Debug.Log($"[RefreshDisplay] Function starts, current column name: {columnName}");
        Debug.Log($"Calling RefreshDisplay() from object: {gameObject.name}, text component: {valueDisplayText}");

        int dayIndex = Mathf.FloorToInt(pinchSlider.SliderValue * 6);
        string value = csvReader.GetValue(dayIndex, columnName);
        Debug.Log($"[Manual Refresh] Day {dayIndex + 1} {columnName} = {value}");

        if (valueDisplayText != null)
        {
            valueDisplayText.text = $"{value}";
            Debug.Log($"Display updated: {valueDisplayText.text}");
        }
        else
        {
            Debug.Log("valueDisplayText is null!");
        }
    }

    public void UpdateBarChartForWeek()
    {
        Debug.Log("[UpdateBarChartForWeek] Function starts");

        if (barChart == null)
        {
            //barChart = barChartObject.GetComponent<BarChart>();
            Debug.Log("Attempting to get barChart at runtime: " + barChart);
        }

        Debug.Log("Current rowData count: " + csvReader.rowData.Count);
        List<float> values = csvReader.GetColumnValues(columnName); // Your newly added function
        Debug.Log($"Values for {columnName}: {string.Join(", ", values)}");

        for (int i = 0; i < Mathf.Min(values.Count, 7); i++) // Only take first 7 days
        {
            string group = $"Day {i + 1}";

            //// Ensure the bar chart has this group
            //if (!barChart.DataSource.HasGroup(group))
            //{
            //    barChart.DataSource.AddGroup(group);
            //}

            // Set or slide to the corresponding value (smooth animation)
            Debug.Log(string.Join(group, values[i]));
            barChart.DataSource.SetValue(group, "All", values[i]);
        }

        Debug.Log("Bar chart updated!");
    }
}
