using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    private bool farmMode;
    private bool useOffset;
    private int currentFarm;
    private int currentScenario;
    private ParticleSystem rain;

    // Table objects
    public GameObject smallTable;
    public GameObject table;
    public GameObject bigTable;
    public GameObject rainObject;

    // Start is called before the first frame update
    void Start()
    {
        rain = rainObject.GetComponent<ParticleSystem>();
        //Debug.Log("Rain system found: " + (rain != null));
        //var emission = rain.emission;
        //emission.enabled = true;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleFarmMode(bool newMode)
    {
        farmMode = newMode;
        UpdateTables();
    }

    public void SetFarm(int newFarm)
    {
        currentFarm = newFarm;
        UpdateTables();
    }

    public void ToggleReuseOffset(bool newOffset)
    {
        useOffset = newOffset;
        table.GetComponent<FarmSimulator>().SetOffset(useOffset);
        smallTable.GetComponent<FarmSimulator>().SetOffset(useOffset);
        bigTable.GetComponent<FarmSimulator>().SetOffset(useOffset);
    }

    // Set the scenario for the tables
    //public void SetScenario(int newScenario)
    //{
    //    currentScenario = newScenario;

    //    // hardcoded logic to toggle rain
    //    if (currentScenario == 0)
    //    {
    //        rain.Stop();
    //    } 
    //    else
    //    {
    //        rain.Play();
    //    }
    //    UpdateTables();
    //}

    public void OnMinimumButtonClick()
    {
        Debug.Log("Minimum button clicked");  // 先确认按钮本身触发了
        SetRainLevel("small");  // 注意这里要和SetRainLevel里的条件匹配
    }

    // Set the scenario for the tables
    public void SetScenario(int newScenario)
    {
        currentScenario = newScenario;

        // hardcoded logic to toggle rain
        if (currentScenario == 0)
        {
            SetRainLevel("small");
        }
        else if (currentScenario == 1)
        {
            SetRainLevel("regular");
        }
        else if (currentScenario == 2)
        {
            SetRainLevel("heavy");
        }

        UpdateTables();
    }

    // Function for raining control
    public void SetRainLevel(string level)
    {
        //粒子系统的发射模块
        var emission = rain.emission;
        //粒子系统的主模块
        var main = rain.main;

        if (level == "small")
        {
            //设置雨滴的发射速率
            emission.rateOverTime = 10;
            //设置雨滴的大小
            main.startSize = 0.05f;
            //rainSound.volume = 0.2f;
            //groundMaterial.SetFloat("_Wetness", 0.2f);
            Debug.Log("Switched to minimum rain");
            rain.Play();
        }
        else if (level == "regular")
        {
            emission.rateOverTime = 100;
            main.startSize = 0.08f;
            //rainSound.volume = 0.5f;
            //groundMaterial.SetFloat("_Wetness", 0.5f);
            Debug.Log("Switched to regular rain");
            rain.Play();
        }
        else if (level == "heavy")
        {
            emission.rateOverTime = 200;
            main.startSize = 0.1f;
            //rainSound.volume = 1.0f;
            //groundMaterial.SetFloat("_Wetness", 1.0f);
            Debug.Log("Switched to heavy rain");
            rain.Play();
        }

    }

    // Reset the state of a table
    private void ResetTable(GameObject table)
    {
        FarmSimulator simulator = table.GetComponent<FarmSimulator>();
        if (simulator != null)
        {
            simulator.SetScenario(currentScenario);
            simulator.Reset();
        }
    }

    public void ResetAllTables()
    {
        ResetTable(smallTable);
        ResetTable(table);
        ResetTable(bigTable);
    }

    // Update the visibility of the tables based on the current farm and mode
    private void UpdateTables()
    {
        if (farmMode)
        {
            if (currentFarm == 0)
            {
                SetTableVisibility(smallTable, true);
                SetTableVisibility(table, false);
                SetTableVisibility(bigTable, false);
                
                
                ResetTable(smallTable);

                //smallTable.transform.localPosition = 3 * Vector3.left + 2 * Vector3.down;
            }
            else if (currentFarm == 1)
            {
                SetTableVisibility(smallTable, false);
                SetTableVisibility(table, true);
                SetTableVisibility(bigTable, false);

                ResetTable(table);
            }
            else if (currentFarm == 2)
            {
                SetTableVisibility(smallTable, false);
                SetTableVisibility(table, false);
                SetTableVisibility(bigTable, true);

                ResetTable(bigTable);
                //bigTable.transform.localPosition = 3 * Vector3.right + 2 * Vector3.down;
            }
        }
        else
        {
            //smallTable.transform.localPosition = 3 * Vector3.left;
            //bigTable.transform.localPosition = 3 * Vector3.right;

            SetTableVisibility(smallTable, true);
            SetTableVisibility(table, true);
            SetTableVisibility(bigTable, true);
            ResetAllTables();
        }
    }

    // Set the visibility of a table (renderers and colliders)
    private void SetTableVisibility(GameObject table, bool isVisible)
    {
        // table.SetActive(isVisible);


        Renderer[] renderers = table.GetComponentsInChildren<Renderer>();
        Collider[] colliders = table.GetComponentsInChildren<Collider>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }

        foreach (Collider collider in colliders)
        {
            collider.enabled = isVisible;
        }
    }
}
