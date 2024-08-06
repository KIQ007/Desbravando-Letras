using UnityEngine;
using UnityEngine.UI;

public class DrawController : MonoBehaviour
{
    public Button toggleButton;
    private FreeDraw freeDraw;

    private void Start()
    {
        freeDraw = FindObjectOfType<FreeDraw>();
        toggleButton.onClick.AddListener(ToggleDrawing);
    }

    private void ToggleDrawing()
    {
        freeDraw.IsDrawing = !freeDraw.IsDrawing;

        if (!freeDraw.IsDrawing)
        {
            freeDraw.ClearDrawing();
        }
    }
}
