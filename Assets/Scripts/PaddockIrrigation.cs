using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PaddockIrrigation : MonoBehaviour
{
    // Enum to represent the state of the paddock
    private enum PaddockState
    {
        Empty,
        Full,
        Saturated,
    }

    private PaddockState currentState = PaddockState.Empty;
    private Material paddockMaterial;
    private float reuseOffset;
    private bool useOffset = true;

    public float animationSpeed = 0.1f;
    public Color basePaddockColor;
    public Color saturatedPaddockColor;

    void Start()
    {
        // Initialize the paddock material and set initial properties
        paddockMaterial = GetComponent<Renderer>().material;
        paddockMaterial.SetFloat("_Panner", 0.0f);
        paddockMaterial.SetFloat("_Drain", 0.0f);
        paddockMaterial.SetColor("_Color", basePaddockColor);

        reuseOffset = paddockMaterial.GetFloat("_ReuseOffset");
    }

    // Coroutine to animate the filling of the paddock
    public IEnumerator AnimateFill()
    {
        paddockMaterial.SetColor("_Color", basePaddockColor);
        paddockMaterial.SetFloat("_Drain", 0.0f);
        yield return AnimatePanner(0.0f, 2.0f);
        currentState = PaddockState.Full;
    }

    // Coroutine to animate the draining of the paddock
    public IEnumerator AnimateDrain()
    {
        paddockMaterial.SetFloat("_Drain", 1.0f);
        yield return AnimatePanner(2.0f, 0.0f);
        currentState = PaddockState.Empty;
    }

    // Coroutine to animate the saturation of the paddock
    //public IEnumerator AnimateSaturation()
    //{
    //    //if (currentState != PaddockState.Full) yield break;
    //    yield return AnimateColor(basePaddockColor, saturatedPaddockColor);
    //    currentState = PaddockState.Saturated;
    //}


    public IEnumerator AnimateSaturation(float tpValue)
    {
        Color targetColor = GetTargetColorFromTP(tpValue);

        paddockMaterial.SetColor("_Color", saturatedPaddockColor);
        paddockMaterial.SetColor("_BaseColor", saturatedPaddockColor);

        Debug.Log($"[AnimateSaturation] Calculated targetColor = {targetColor}");
        yield return AnimateColor(basePaddockColor, targetColor);
        currentState = PaddockState.Saturated;
    }

    // Public method to reset the paddock state
    public void Reset()
    {
        currentState = PaddockState.Empty;
        paddockMaterial.SetFloat("_Panner", 0.0f);
        paddockMaterial.SetFloat("_Drain", 0.0f);
        paddockMaterial.SetColor("_Color", basePaddockColor);
    }

    // Private method to animate the panner property of the material
    private IEnumerator AnimatePanner(float initialPanner, float targetPanner)
    {
        float elapsedTime = 0f;
        float duration = Mathf.Abs(targetPanner - initialPanner) / animationSpeed;


        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            paddockMaterial.SetFloat("_Panner", Mathf.Lerp(initialPanner, targetPanner, t));
            yield return null;
        }
    }

    // Private method to animate the color property of the material
    private IEnumerator AnimateColor(Color initialColor, Color targetColor)
    {
        float elapsedTime = 0f;
        float duration = 1f / animationSpeed;

        Debug.Log($"[AnimateColor] From {initialColor} to {targetColor} over {duration:F2}s");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            paddockMaterial.SetColor("_Color", Color.Lerp(initialColor, targetColor, t));

            yield return null;
        }
    }

    // Public method to set the reuse offset of the paddock
    public void SetOffset(bool newUseOffset)
    {
        useOffset = newUseOffset;
        paddockMaterial.SetFloat("_ReuseOffset", useOffset ? reuseOffset : 0.0f);
    }

    //Public method to set the saturation of the paddock based on a TP value
    private Color GetTargetColorFromTP(float tpValue)
    {
        float t = Mathf.Clamp01(tpValue / 8f);

        Color cleanColor = new Color(0.71f, 1f, 0.71f);
        Color pollutedColor = new Color(0.51f, 0.4f, 0.31f);
        Color result = Color.Lerp(cleanColor, pollutedColor, t);

        saturatedPaddockColor = result;

        Debug.Log($"[TP Color] TP={tpValue}, Normalized={t}, Result={result}");


        return saturatedPaddockColor;
    }
    public void SetTP(float tpValue)
    {
        Color newColor = GetTargetColorFromTP(tpValue);
        saturatedPaddockColor = newColor;

        paddockMaterial.SetColor("_Color", newColor);
        paddockMaterial.SetColor("_BaseColor", newColor);
    }



}
