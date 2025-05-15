using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class ShowSliderDayValue : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro textMesh = null;

    [SerializeField]
    private int maxDay = 7;
    public GameObject sliderPanel;

    private bool isDragging = false;  // 👈 Dragging state flag

    private void Awake()
    {
        Debug.Log("[Debug] Awake called");

        // Automatically try to find the TextMeshPro component
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
            Debug.Log("[Debug] Auto-acquired TextMeshPro: " + (textMesh != null ? "Success" : "Failed"));
        }

        // ✅ Add listeners for drag start and end
        var slider = sliderPanel.GetComponentInChildren<PinchSlider>();
        if (slider == null)
        {
            Debug.LogError("[Debug] PinchSlider not found!");
        }
        else
        {
            Debug.Log("[Debug] Found PinchSlider: " + slider.gameObject.name);
        }

        if (slider != null)
        {
            slider.OnInteractionStarted.AddListener((e) =>
            {
                isDragging = true;
                Debug.Log("[Debug] Drag started, isDragging = true");
            });

            slider.OnInteractionEnded.AddListener((e) =>
            {
                isDragging = false;
                Debug.Log("[Debug] Drag ended, isDragging = false");
            });

            Debug.Log("[Debug] Successfully registered drag listeners");
        }
    }

    public void OnSliderUpdated(SliderEventData eventData)
    {
        if (textMesh == null)
        {
            Debug.LogError("[Debug] textMesh is not assigned!");
            return;
        }

        if (eventData == null)
        {
            Debug.LogError("[Debug] eventData is null!");
            return;
        }

        float rawValue = eventData.NewValue;  // Typically between 0 and 1
        int dayIndex = Mathf.FloorToInt(rawValue * (maxDay - 1));

        Debug.Log($"[Debug] Raw slider value: {rawValue}, calculated dayIndex: {dayIndex}");

        textMesh.text = $"Day {dayIndex + 1}";
    }

    public void SetMaxDay(int newMaxDay)
    {
        maxDay = newMaxDay;
        Debug.Log($"[Debug] maxDay updated to: {maxDay}");
    }

    public void ToggleSliderPanel()
    {
        // Prevent hiding or showing the slider panel while dragging
        if (isDragging)
        {
            Debug.LogWarning("Currently dragging the slider, toggle action is blocked!");
            return;
        }

        sliderPanel.SetActive(!sliderPanel.activeSelf);
    }
}
