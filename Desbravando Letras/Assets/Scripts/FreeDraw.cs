using System.Collections.Generic;
using UnityEngine;

public class FreeDraw : MonoBehaviour
{
    public float lineWidth = 0.1f;
    public Color lineColor = Color.red;
    public Color collisionLineColor = Color.green;

    private LineRenderer currentLine;
    private List<Vector3> points;
    private Color originalColor;

    public bool IsDrawing { get; set; } = false;
    public bool IsTouchingA { get; set; } = false;
    public bool IsTouchingB { get; set; } = false;

    private LetterManager letterManager;

    private void Start()
    {
        points = new List<Vector3>();
        originalColor = lineColor;
        letterManager = FindObjectOfType<LetterManager>();
    }

    private void Update()
    {
        if (IsDrawing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startPosition.z = 0f;

                // Verifique a colisão no início do desenho
                IsTouchingA = CheckLineCollision(startPosition, "A");
                IsTouchingB = CheckLineCollision(startPosition, "B");

                StartNewLine(IsTouchingA || IsTouchingB ? collisionLineColor : originalColor, startPosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentPosition.z = 0f;

                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], currentPosition) > 0.0001f)
                {
                    bool currentlyTouchingA = CheckLineCollision(currentPosition, "A");
                    bool currentlyTouchingB = CheckLineCollision(currentPosition, "B");

                    if (currentlyTouchingA && !IsTouchingA)
                    {
                        IsTouchingA = true;
                        StartNewLine(collisionLineColor, currentPosition);
                    }
                    else if (!currentlyTouchingA && IsTouchingA)
                    {
                        IsTouchingA = false;
                        StartNewLine(originalColor, currentPosition);
                    }
                    else if (currentlyTouchingB && !IsTouchingB)
                    {
                        IsTouchingB = true;
                        StartNewLine(collisionLineColor, currentPosition);
                    }
                    else if (!currentlyTouchingB && IsTouchingB)
                    {
                        IsTouchingB = false;
                        StartNewLine(originalColor, currentPosition);
                    }
                    else
                    {
                        AddPointToLine(currentPosition);
                    }

                    if (points.Count > 1)
                    {
                        letterManager.UpdatePaintedArea(points[points.Count - 2], currentPosition, currentlyTouchingA, currentlyTouchingB, lineWidth);
                    }
                }
            }
        }
    }

    private bool CheckLineCollision(Vector3 position, string tag)
    {
        Collider2D collider = Physics2D.OverlapPoint(position);
        return collider != null && collider.CompareTag(tag);
    }

    public void StartNewLine(Color color, Vector3 startPosition)
    {
        if (currentLine != null)
        {
            currentLine = null;
        }

        GameObject lineObject = new GameObject("Line");
        currentLine = lineObject.AddComponent<LineRenderer>();
        lineObject.tag = "Line";

        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        currentLine.startColor = color;
        currentLine.endColor = color;
        currentLine.material = new Material(Shader.Find("Sprites/Default"));
        currentLine.sortingOrder = 1;

        points.Clear();
        points.Add(startPosition);
        currentLine.positionCount = points.Count;
        currentLine.SetPosition(points.Count - 1, startPosition);
    }

    public void AddPointToLine(Vector3 position)
    {
        if (currentLine != null)
        {
            points.Add(position);
            currentLine.positionCount = points.Count;
            currentLine.SetPosition(points.Count - 1, position);
        }
    }

    public void ClearDrawing()
    {
        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }

        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }

        points.Clear();
    }
}
