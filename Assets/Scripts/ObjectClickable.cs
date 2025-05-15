using UnityEngine;

public class ObjectClickable : MonoBehaviour
{
    public MenuIntroController menuIntro;

    //public string farmSize = "5ML";
    //public string scenario = "LightRainfall";
    //public string targetName = "IrrigationChannel";

    [TextArea] public string title = "Irrigation Delivery Channel";
    [TextArea] public string description = "carries water pumped by farmers from the reservoir.";
    public string unit = "mm";

    [Header("Highlight Visual")]
    private bool isHighlighted = false;
    public Material normalMaterial;
    public Material highlightMaterial;
    private Renderer objRenderer;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        objRenderer.material = normalMaterial;
    }

    public void OnClicked()
    {
        Debug.Log("🟡 ObjectClickable.OnClicked() is activated！");

        isHighlighted = !isHighlighted;


        if (isHighlighted)
        {
            // show DataBoard
            menuIntro.ShowDashboard(menuIntro.GetCurrentFarmSize(),
                                    menuIntro.GetCurrentScenario(),
                                    menuIntro.GetCurrentTarget(),
                                    title,
                                    description,
                                    unit);
            objRenderer.material = highlightMaterial;
        }
        else
        {
            // close DataBoard
            menuIntro.dataBoard.SetActive(false);
            objRenderer.material = normalMaterial;
        }

    }
}
