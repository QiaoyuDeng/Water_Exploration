using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using ChartAndGraph;

public class FarmSimulator : MonoBehaviour
{

    // Public variables for the game objects
    public GameObject irrigationWaterPlane;
    public GameObject reuseWaterPlane;
    public GameObject effluentWaterPlane;
    public GameObject paddock;
    public GameObject overflow;
    public GameObject overflowWater;
    public GameObject reusePipe;
    public GameObject shedPipe;
    public GameObject effluentPipe;

    // Pipe materials
    private Material reusePipeMaterial;
    private Material shedPipeMaterial;
    private Material effluentPipeMaterial;

    // Animation variables
    private AnimatePlane irrigationWaterPlaneScript;
    private AnimatePlane reuseWaterPlaneScript;
    private AnimatePlane effluentWaterPlaneScript;
    private PaddockIrrigation paddockScript;
    private ParticleSystem overflowParticles;
    private OverflowIrrigation overflowScript;

    private MaterialAnimator reusePipeAnimator;
    private MaterialAnimator shedPipeAnimator;
    private MaterialAnimator effluentPipeAnimator;

    private CowFactory cowFactory;
    private int currentScenario = 0;

    // Data input
    public CSVReader csvReader;  // csv reader
    private float reuseStorageVolume = 0;
    private float reuseOverflowPlux = 0;
    private float pumpBackReuseRate = 0;

    // Indicates whether this farm has completed the current simulation step
    public bool isReadyForNext = true;


    // narrative manager
    public PlayNarrationManager narrationManager; // NarrationManager
    public int currentScenarioId = 0;         // 0=light, 1=moderate, 2=heavy
    public int currentDay = 0;                // 0~6
    private Coroutine currentAnimation;

    [Header("Overflow Visual")]
    //public Transform overflowCube;
    public Transform overflowBase;         
    public Transform overflowMiddle;      
    public Transform cubeTopAnchor;       
    public GameObject overflowTooltip; // Tooltip root object that contains the label and visuals
    private float anchorBaseY;

    public float heightScale = 10f; // adjust this value to change the height of the overflow cube
    private float totalOverflow = 0f;
    public TMPro.TextMeshPro overflowLabel;

    [Header("Paddock Saturation")]
    public float paddockTPValue = 0.0f; // paddock TP value for the current scenario

    [Header("Other Visual Data")]
    public GameObject ReuseFillTextGroup;
    public GameObject OverflowPercentGroup;
    public GameObject TotalPGroup;
    public GameObject TotalPPercentGroup;



    // Start is called before the first frame update
    void Start()
    {
        // Get the scripts from the game objects
        irrigationWaterPlaneScript = irrigationWaterPlane.GetComponent<AnimatePlane>();
        reuseWaterPlaneScript = reuseWaterPlane.GetComponent<AnimatePlane>();
        effluentWaterPlaneScript = effluentWaterPlane.GetComponent<AnimatePlane>();
        paddockScript = paddock.GetComponent<PaddockIrrigation>();
        overflowParticles = overflow.GetComponent<ParticleSystem>();
        overflowScript = overflowWater.GetComponent<OverflowIrrigation>();
        overflowParticles.Stop();

        //Get materials from pipes and create instances
        reusePipeMaterial = reusePipe.GetComponent<Renderer>().material;
        shedPipeMaterial = shedPipe.GetComponent<Renderer>().material;
        effluentPipeMaterial = effluentPipe.GetComponent<Renderer>().material;

        // Initialize material animators
        reusePipeAnimator = new MaterialAnimator(reusePipeMaterial, "_Panner");
        shedPipeAnimator = new MaterialAnimator(shedPipeMaterial, "_Panner");
        effluentPipeAnimator = new MaterialAnimator(effluentPipeMaterial, "_Panner");
        
        //Cow factory
        cowFactory = GetComponent<CowFactory>();

        if (cowFactory != null)
        {
            cowFactory.SpawnCows();
        }

        anchorBaseY = cubeTopAnchor.localPosition.y;

        // Start the animation for the desired scenario
        // StartCoroutine(ChooseScenario(currentScenario));
    }

    // Update is called once per frame
    //void Update()
    //{
    //    paddockScript.SetTP(paddockTPValue);
    //}

    // Scenario 1: Normal weather conditions with optimum storage condition
    private IEnumerator AnimateNormalWeather()
    {
        // Use multiple subroutines to animate the farm
        while (true)
        {
            isReadyForNext = false;

            // 1. fill the irrigation water plane while stopping reuse
            yield return irrigationWaterPlaneScript.ChangeVolumeByAmount(500);
            reusePipeAnimator.StopAnimation(this);
            effluentPipeAnimator.StopAnimation(this);

            // 2. fill the paddock while draining the irrigation water plane
            StartCoroutine(irrigationWaterPlaneScript.MovePlane(0));
            yield return StartCoroutine(paddockScript.AnimateFill());

            yield return cowFactory.MoveToPlane(paddock);

            // 3. saturate the paddock
            yield return StartCoroutine(paddockScript.AnimateSaturation(paddockTPValue));

            yield return cowFactory.ReturnCows();

            // 4. fill the reuse water plane while draining the paddock and no overflow due to sufficient storage
            //StartCoroutine(paddockScript.AnimateDrain());
            //yield return StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(900));
            StartCoroutine(paddockScript.AnimateDrain());
            yield return StartCoroutine(reuseWaterPlaneScript.SetVolumeAndMove(reuseStorageVolume, reuseOverflowPlux));

            // 5. fill the effluent water plane
            shedPipeAnimator.StartAnimation(this);
            yield return StartCoroutine(effluentWaterPlaneScript.ChangeVolumeByAmount(500));
            shedPipeAnimator.StopAnimation(this);

            // 6. Pump water back to irrigation (effluent + pump rate of reuse)

            StartCoroutine(effluentWaterPlaneScript.MovePlane(0));
            StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(-300));
            reusePipeAnimator.StartAnimation(this);
            effluentPipeAnimator.StartAnimation(this);
            //overflowParticles.Stop();
            yield return StartCoroutine(irrigationWaterPlaneScript.ChangeVolumeByAmount(1000));

            isReadyForNext = true;
        }
        
    }

    // Scenario 2: Heavy rainfall with overflow
    private IEnumerator AnimateHeavyRainfall()
    {
        while (true)
        {
            isReadyForNext = false;

            // 1. fill the irrigation water plane
            //reusePipeAnimator.StartAnimation(this);
            yield return irrigationWaterPlaneScript.ChangeVolumeByAmount(1900);
            reusePipeAnimator.StopAnimation(this);
            effluentPipeAnimator.StopAnimation(this);

            // 2. fill the paddock while draining the irrigation water plane
            StartCoroutine(irrigationWaterPlaneScript.MovePlane(0));
            yield return StartCoroutine(paddockScript.AnimateFill());

            yield return cowFactory.MoveToPlane(paddock);

            // 3. saturate the paddock
            yield return StartCoroutine(paddockScript.AnimateSaturation(paddockTPValue));

            yield return cowFactory.ReturnCows();

            // 4. fill the reuse water plane while draining the paddock and overflow due to heavy rainfall
            //StartCoroutine(paddockScript.AnimateDrain());
            //yield return StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(1200));
            StartCoroutine(paddockScript.AnimateDrain());
            yield return StartCoroutine(reuseWaterPlaneScript.SetVolumeAndMove(reuseStorageVolume, reuseOverflowPlux));

            // 5. fill the effluent water plane 
            shedPipeAnimator.StartAnimation(this);
            yield return StartCoroutine(effluentWaterPlaneScript.ChangeVolumeByAmount(500));
            shedPipeAnimator.StopAnimation(this);

            // 6. Pump water back to irrigation (effluent + pump rate of reuse)
            StartCoroutine(effluentWaterPlaneScript.MovePlane(0));
            StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(-600));
            reusePipeAnimator.StartAnimation(this);
            effluentPipeAnimator.StartAnimation(this);
            yield return StartCoroutine(irrigationWaterPlaneScript.ChangeVolumeByAmount(600));

            isReadyForNext = true;
        }
    }

    // Scenario 3: Drought conditions with limited water
    private IEnumerator AnimateDroughtConditions()
    {
        while (true)
        {
            isReadyForNext = false;
            
            // 1. fill the irrigation water plane with limited water
            //reusePipeAnimator.StartAnimation(this);
            yield return irrigationWaterPlaneScript.ChangeVolumeByAmount(500);
            reusePipeAnimator.StopAnimation(this);
            effluentPipeAnimator.StopAnimation(this);

            // 2. fill the paddock while draining the irrigation water plane
            StartCoroutine(irrigationWaterPlaneScript.MovePlane(0));
            yield return StartCoroutine(paddockScript.AnimateFill());

            yield return cowFactory.MoveToPlane(paddock);

            // 3. saturate the paddock
            yield return StartCoroutine(paddockScript.AnimateSaturation(paddockTPValue));

            yield return cowFactory.ReturnCows();

            // 4. fill the reuse water plane while draining the paddock with limited water
            //StartCoroutine(paddockScript.AnimateDrain());
            //yield return StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(500));
            StartCoroutine(paddockScript.AnimateDrain());
            yield return StartCoroutine(reuseWaterPlaneScript.SetVolumeAndMove(reuseStorageVolume, reuseOverflowPlux));

            // 5. fill the effluent water plane with limited water
            shedPipeAnimator.StartAnimation(this);
            yield return StartCoroutine(effluentWaterPlaneScript.ChangeVolumeByAmount(300));
            shedPipeAnimator.StopAnimation(this);

            // 6. Pump water back to irrigation (effluent + pump rate of reuse)
            StartCoroutine(effluentWaterPlaneScript.MovePlane(0));
            StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(-500));
            reusePipeAnimator.StartAnimation(this);
            effluentPipeAnimator.StartAnimation(this);
            yield return StartCoroutine(irrigationWaterPlaneScript.ChangeVolumeByAmount(500));
            yield return new WaitForSeconds(3f);

            isReadyForNext = true;
        }
    }

    /// <summary>
    /// animate scenario 
    /// </summary>
    /// <returns></returns>
    public IEnumerator AnimateScenario()
    {

        isReadyForNext = false;

        yield return StartCoroutine(WaitForBoth(
            RunFullAnimationSequence(),
            narrationManager.PlayNarrationAndWait(currentScenarioId, currentDay, 0)
        ));

        isReadyForNext = true;
    }

    public IEnumerator AnimateScenarioSilent()
    {

        isReadyForNext = false;
        Debug.Log($"{this.name}: Running animation");
        currentAnimation = StartCoroutine(RunFullAnimationSequence());
        yield return currentAnimation;
        isReadyForNext = true;
    }


    private IEnumerator RunFullAnimationSequence()
    {
        Debug.Log($"{this.name}: Running animation");

        // Step 1
        yield return irrigationWaterPlaneScript.ChangeVolumeByAmount(500);
        reusePipeAnimator.StopAnimation(this);
        effluentPipeAnimator.StopAnimation(this);

        // Step 2
        StartCoroutine(irrigationWaterPlaneScript.MovePlane(0));
        yield return paddockScript.AnimateFill();
        yield return cowFactory.MoveToPlane(paddock);

        // Step 3
        yield return paddockScript.AnimateSaturation(paddockTPValue);
        yield return cowFactory.ReturnCows();

        // Step 4
        StartCoroutine(paddockScript.AnimateDrain());
        yield return reuseWaterPlaneScript.SetVolumeAndMove(reuseStorageVolume, reuseOverflowPlux);

        // Step 5
        shedPipeAnimator.StartAnimation(this);
        yield return effluentWaterPlaneScript.ChangeVolumeByAmount(300);
        shedPipeAnimator.StopAnimation(this);

        // Step 6
        StartCoroutine(effluentWaterPlaneScript.MovePlane(0));
        StartCoroutine(reuseWaterPlaneScript.ChangeVolumeByAmount(pumpBackReuseRate));
        reusePipeAnimator.StartAnimation(this);
        effluentPipeAnimator.StartAnimation(this);
        yield return irrigationWaterPlaneScript.ChangeVolumeByAmount(-pumpBackReuseRate);
        yield return new WaitForSeconds(3f);
    }

    public void StopAllAnimations()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        isReadyForNext = true;
        Debug.Log(name + " animations stopped.");
    }


    // Reset the farm simulator to the initial state
    public void Reset()
    {
        cowFactory.DestroyCows();

        irrigationWaterPlaneScript.Reset();
        reuseWaterPlaneScript.Reset();
        effluentWaterPlaneScript.Reset();
        paddockScript.Reset();
        overflowParticles.Stop();

        reusePipeAnimator.StopAnimation(this);
        shedPipeAnimator.StopAnimation(this);
        effluentPipeAnimator.StopAnimation(this);

        StopAllCoroutines();

        //get any renderer
        Renderer renderer = paddock.GetComponent<Renderer>();

        //If renderer is enabled
        if (renderer.enabled)
        {
            cowFactory.SpawnCows();
        }

        //Restart the chosen scenario
        StartCoroutine(ChooseScenario(currentScenario));
    }

    public void SetScenario(int newScenario)
    {
        currentScenario = newScenario;
    }

    // Method to choose the scenario based on the value
    private IEnumerator ChooseScenario(int value)
    {
        if (value == 0)
        {
            return AnimateDroughtConditions();
        }
        else if (value == 1)
        {
            return AnimateNormalWeather();
        }
        else if (value == 2)
        {
            return AnimateHeavyRainfall();
        }
        else
        {
            return AnimateNormalWeather();
        }
    }
    
    public void SetOffset(bool newUseOffset)
    {
        paddockScript.SetOffset(newUseOffset);
        overflowScript.SetOffset(newUseOffset);
    }
        

    private IEnumerator TestMaterialAnimation()
    {
        reusePipeAnimator.StartAnimation(this);
        shedPipeAnimator.StartAnimation(this);
        effluentPipeAnimator.StartAnimation(this);
        yield return new WaitForSeconds(5f);
        reusePipeAnimator.StopAnimation(this);
        shedPipeAnimator.StopAnimation(this);
        effluentPipeAnimator.StopAnimation(this);
    }

    // Method to get a random point on the plane
    public Vector3 GetRandomPointOnPlane(GameObject plane)
    {
        // Get the plane's mesh renderer bounds
        Bounds planeBounds = plane.GetComponent<MeshRenderer>().bounds;
        
        // Generate random point within bounds
        float randomX = Random.Range(planeBounds.min.x, planeBounds.max.x);
        float randomZ = Random.Range(planeBounds.min.z, planeBounds.max.z);
        
        // Use the plane's Y position
        float planeY = plane.transform.position.y;
        
        return new Vector3(randomX, planeY, randomZ);
    }

    ////////////////////////////////////////////////////////////
    // player can call this method to run a step of irrigation
    public void SetReuseValues(float volume, float overflow, float cumulativeOverflow)
    {
        reuseStorageVolume = volume;
        reuseOverflowPlux = overflow;
        //totalOverflow += overflow;
        totalOverflow = cumulativeOverflow;


        float height = totalOverflow * heightScale;

        // Ensure the top anchor is active and moved to correct height
        if (cubeTopAnchor != null)
        {
            if (!cubeTopAnchor.gameObject.activeSelf)
                cubeTopAnchor.gameObject.SetActive(true);

            Vector3 anchorPos = cubeTopAnchor.localPosition;
            anchorPos.y = anchorBaseY + height; // correct starting offset 
            cubeTopAnchor.localPosition = anchorPos;
        }

        // Enable and adjust the middle pillar to visually connect base and top
        if (overflowMiddle != null && overflowBase != null)
        {
            if (!overflowMiddle.gameObject.activeSelf)
                overflowMiddle.gameObject.SetActive(true);

            // Compute distance between base and top
            float topY = cubeTopAnchor.localPosition.y;
            float baseY = overflowBase.localPosition.y;
            float distance = topY - baseY;

            Debug.Log($"[Overflow] Top Y: {topY}, Base Y: {baseY}, Distance: {distance}");

            // Position the middle pillar halfway between base and top
            Vector3 midPos = overflowMiddle.localPosition;
            midPos.y = baseY + distance / 2f;
            overflowMiddle.localPosition = midPos;
            Debug.Log($"[Overflow] Middle Position Y set to: {midPos.y}");

            // Scale the middle pillar so its height matches the distance (pivot at center)
            Vector3 midScale = overflowMiddle.localScale;
            midScale.y = distance;
            overflowMiddle.localScale = midScale;
            Debug.Log($"[Overflow] Middle Scale Y set to: {midScale.y}");
        }

        // Show and position the tooltip root (overflowTooltip)
        if (overflowTooltip != null)
        {
            if (!overflowTooltip.activeSelf) 
            {
                overflowLabel.text = "<b><size=80>Overflow Phosphorus:</size></b>\n<b><size=80>0.00 kg</size></b>";
                overflowTooltip.SetActive(true);
            }
                

            // Place the tooltip at the top anchor's world position
            overflowTooltip.transform.position = cubeTopAnchor.position;
        }

        // Update tooltip text
        if (overflowLabel != null)
        {
            overflowLabel.text = "<b><size=80>Overflow Phosphorus</size></b>\n<b><size=80>" + Mathf.RoundToInt(totalOverflow) + " kg</size></b>";
            overflowLabel.fontSize = 80; 


            // Do NOT manually set overflowLabel.transform.position — it follows the tooltip root
        }
    }


    // new method to reset the overflow cube and label
    public void ResetOverflowCube()
    {
        // Reset the total overflow to 0
        totalOverflow = 0f;

        // Hide the middle pillar
        if (overflowMiddle != null)
        {
            overflowMiddle.gameObject.SetActive(false);
        }

        // Reset the top anchor point to a small Y value
        if (cubeTopAnchor != null)
        {
            Vector3 pos = cubeTopAnchor.localPosition;
            pos.y = anchorBaseY; // This ensures it's not at ground level or completely hidden
            cubeTopAnchor.localPosition = pos;
        }

        // Reset the tooltip label text
        if (overflowLabel != null)
        {
            overflowLabel.text = "<b><size=80>Overflow Phosphorus:</size></b>\n<b><size=80>0.00 kg</size></b>";
            overflowLabel.fontSize = 80;
        }

        // Hide the tooltip root object
        if (overflowTooltip != null)
        {
            overflowTooltip.SetActive(false);
        }
    }

    public void HideOverflowObjects()
    {
        if (overflowBase != null)
            overflowBase.gameObject.SetActive(false);

        if (overflowMiddle != null)
            overflowMiddle.gameObject.SetActive(false);

        if (cubeTopAnchor != null)
            cubeTopAnchor.gameObject.SetActive(false);

        if (overflowTooltip != null)
            overflowTooltip.SetActive(false);

        if (ReuseFillTextGroup != null)
            ReuseFillTextGroup.SetActive(false);

        if (OverflowPercentGroup != null)
            OverflowPercentGroup.SetActive(false);

        if (TotalPGroup != null)
            TotalPGroup.SetActive(false);

        if (TotalPPercentGroup != null)
            TotalPPercentGroup.SetActive(false);
    }


    public void SetPumpBackReuseRate(float rate)
    {
        pumpBackReuseRate = rate;
    }
    ////////////////////////////////////////////////////////////

    private IEnumerator WaitForBoth(IEnumerator a, IEnumerator b)
    {
        bool aDone = false, bDone = false;
        StartCoroutine(RunAndFlag(a, () => aDone = true));
        StartCoroutine(RunAndFlag(b, () => bDone = true));
        while (!aDone || !bDone)
        {
            yield return null;
        }
    }

    private IEnumerator RunAndFlag(IEnumerator coroutine, System.Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    private IEnumerator RunBoth(IEnumerator a, IEnumerator b)
    {
        bool aDone = false, bDone = false;
        StartCoroutine(RunAndFlag(a, () => aDone = true));
        StartCoroutine(RunAndFlag(b, () => bDone = true));
        while (!aDone || !bDone)
        {
            yield return null;
        }
    }



}



// MaterialAnimator class to animate material properties
public class MaterialAnimator
{
    private Material material;
    private string propertyName;
    private Coroutine animationCoroutine;

    public MaterialAnimator(Material material, string propertyName)
    {
        this.material = material;
        this.propertyName = propertyName;
        ResetMaterial();
    }

    public void StartAnimation(MonoBehaviour owner)
    {
        if (animationCoroutine != null)
        {
            owner.StopCoroutine(animationCoroutine);
        }
        animationCoroutine = owner.StartCoroutine(AnimateMaterial());
    }

    public void StopAnimation(MonoBehaviour owner)
    {
        if (animationCoroutine != null)
        {
            owner.StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        ResetMaterial();
    }

    private IEnumerator AnimateMaterial()
    {
        float value = -0.5f;
        while (true)
        {
            value += Time.deltaTime;
            if (value > 1.5f)
            {
                value = -0.5f;
            }
            material.SetFloat(propertyName, value);
            yield return null;
        }
    }

    private void ResetMaterial()
    {
        material.SetFloat(propertyName, -0.5f);
    }



}



