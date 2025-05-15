using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEngine.Rendering.DebugUI.Table;

public class CSVReader : MonoBehaviour
{
    public TextAsset csvFile;

    public List<string> headers = new List<string>();
    public List<Dictionary<string, string>> rowData = new List<Dictionary<string, string>>();

    public void ReadCSV()
    {
        rowData.Clear();
        headers.Clear();

        if (csvFile == null)
        {
            Debug.LogWarning("CSV file not assigned.");
            return;
        }

        StringReader reader = new StringReader(csvFile.text);
        string line;

        // Get headers
        if ((line = reader.ReadLine()) != null)
        {
            headers.AddRange(line.Split(','));
        }

        // Get data
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');
            Dictionary<string, string> entry = new Dictionary<string, string>();

            Debug.Log("Headers: " + string.Join(",", headers));

            for (int i = 0; i < headers.Count && i < values.Length; i++)
            {
                entry[headers[i]] = values[i];
            }

            rowData.Add(entry);
        }

    }

    // Get value by column name and row index
    public string GetValue(int rowIndex, string columnName)
    {
        if (rowIndex >= 0 && rowIndex < rowData.Count && rowData[rowIndex].ContainsKey(columnName))
        {
            return rowData[rowIndex][columnName];
        }
        return null;
    }

    // Get all values of a column (for bar chart)
    public List<float> GetColumnValues(string columnName)
    {
        Debug.Log("Game started");
        
        List<float> values = new List<float>();

        foreach (var row in rowData)
        {
            Debug.Log("Current row keys:" + string.Join(", ", row.Keys));
            if (row.ContainsKey(columnName))
            {
                string raw = row[columnName];
                Debug.Log($"Raw value: {raw}");

                if (float.TryParse(row[columnName], out float val))
                {
                    values.Add(val);
                    Debug.Log($"Conversion successful: {val}");
                }
                else
                {
                    Debug.LogWarning($"Conversion failed: {raw}");
                    values.Add(0); 
                }
            }
        }

        return values;
    }

}