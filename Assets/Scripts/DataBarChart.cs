using UnityEngine;
using System.Collections;
using ChartAndGraph;

public class DataBarChart : MonoBehaviour {
	void Start () {
        BarChart barChart = GetComponent<BarChart>();
        if (barChart != null)
        {
            barChart.DataSource.SetValue("Player 1", "All", 1.2);
            barChart.DataSource.SetValue("Player 2", "All", 0.57);
            //barChart.DataSource.SlideValue("Player 2", "Value 1", Random.value * 20, 40f);
        }
    }
    private void Update()
    {
    }
}
