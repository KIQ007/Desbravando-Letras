using UnityEngine;
using UnityEngine.UI;

public class LetterManager : MonoBehaviour
{
    public GameObject letterObjectA;
    public GameObject letterObjectB;
    public float requiredPercentage = 60f;
    public Text percentageText;

    private Collider2D letterColliderA;
    private Collider2D letterColliderB;
    private float totalPaintedAreaA = 0f;
    private float totalPaintedAreaB = 0f;

    private FreeDraw freeDraw;

    private void Start()
    {
        freeDraw = FindObjectOfType<FreeDraw>();

        letterColliderA = letterObjectA.GetComponent<Collider2D>();
        letterObjectB.SetActive(false);
        letterColliderB = letterObjectB.GetComponent<Collider2D>();

        if (percentageText != null)
        {
            percentageText.text = "A: 0%";
        }
    }

    private void Update()
    {
        if (freeDraw.IsDrawing)
        {
            CheckPaintingProgress();
        }
    }

    private void CheckPaintingProgress()
    {
        if (letterColliderA != null)
        {
            float percentagePaintedA = (totalPaintedAreaA / CalculateLetterArea(letterColliderA)) * 100f;
            if (percentageText != null && letterObjectA.activeSelf)
            {
                percentageText.text = "A: " + percentagePaintedA.ToString("F2") + "%";
            }

            if (percentagePaintedA >= requiredPercentage && letterObjectA.activeSelf)
            {
                letterObjectA.SetActive(false);
                letterObjectB.SetActive(true);
                freeDraw.ClearDrawing();
                UpdatePercentageText();
            }
        }

        if (letterColliderB != null)
        {
            float percentagePaintedB = (totalPaintedAreaB / CalculateLetterArea(letterColliderB)) * 100f;
            if (percentageText != null && letterObjectB.activeSelf)
            {
                percentageText.text = "B: " + percentagePaintedB.ToString("F2") + "%";
            }

            if (percentagePaintedB >= requiredPercentage && letterObjectB.activeSelf)
            {
                letterObjectB.SetActive(false);
                freeDraw.ClearDrawing();
                UpdatePercentageText();
            }
        }
    }

    private void UpdatePercentageText()
    {
        if (letterObjectA.activeSelf)
        {
            float percentagePaintedA = (totalPaintedAreaA / CalculateLetterArea(letterColliderA)) * 100f;
            percentageText.text = "A: " + percentagePaintedA.ToString("F2") + "%";
        }
        else if (letterObjectB.activeSelf)
        {
            float percentagePaintedB = (totalPaintedAreaB / CalculateLetterArea(letterColliderB)) * 100f;
            percentageText.text = "B: " + percentagePaintedB.ToString("F2") + "%";
        }
        else
        {
            percentageText.text = "";
        }
    }

    private float CalculateLetterArea(Collider2D collider)
    {
        if (collider == null) return 0f;
        Bounds bounds = collider.bounds;
        return bounds.size.x * bounds.size.y;
    }

    public void UpdatePaintedArea(Vector3 lastPoint, Vector3 currentPoint, bool touchingA, bool touchingB, float lineWidth)
    {
        if (touchingA)
        {
            totalPaintedAreaA += Vector3.Distance(lastPoint, currentPoint) * lineWidth;
        }
        if (touchingB)
        {
            totalPaintedAreaB += Vector3.Distance(lastPoint, currentPoint) * lineWidth;
        }
    }
}
