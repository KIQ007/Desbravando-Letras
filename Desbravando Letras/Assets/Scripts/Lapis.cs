using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreeDraw : MonoBehaviour
{
    private LineRenderer currentLine;
    private List<Vector3> points;
    [SerializeField]
    private float lineWidth = 0.1f; // Largura da linha
    [SerializeField]
    private Color lineColor = Color.red; // Cor da linha
    [SerializeField]
    private Color collisionLineColor = Color.green; // Cor da linha em colisão
    private Color originalColor; // Cor original da linha

    public Button toggleButton; // Botão para ativar/desativar o desenho

    private bool isDrawing = false; // Variável para controlar se está desenhando ou não
    private bool isTouchingA = false; // Variável para controlar se a ponta da linha está tocando um objeto com a tag "A"
    private bool isTouchingB = false; // Variável para controlar se a ponta da linha está tocando um objeto com a tag "B"

    private Collider2D letterColliderA; // Collider da letra A
    private Collider2D letterColliderB; // Collider da letra B
    private GameObject letterObjectA; // Objeto da letra A
    private GameObject letterObjectB; // Objeto da letra B

    [SerializeField]
    private float requiredPercentage = 60f; // Porcentagem requerida de pintura

    private float totalPaintedAreaA = 0f; // Área total pintada em A
    private float totalPaintedAreaB = 0f; // Área total pintada em B

    void Start()
    {
        points = new List<Vector3>();
        originalColor = lineColor;

        // Associe a função ToggleDrawing ao botão
        toggleButton.onClick.AddListener(ToggleDrawing);

        // Encontre o objeto com a tag "A" e obtenha seu collider
        letterObjectA = GameObject.FindGameObjectWithTag("A");
        if (letterObjectA != null)
        {
            letterColliderA = letterObjectA.GetComponent<Collider2D>();
        }

        // Encontre o objeto com a tag "B" e oculte-o inicialmente
        letterObjectB = GameObject.FindGameObjectWithTag("B");
        if (letterObjectB != null)
        {
            letterColliderB = letterObjectB.GetComponent<Collider2D>();
            letterObjectB.SetActive(false);
        }
    }

    void Update()
    {
        if (isDrawing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startPosition.z = 0f;
                bool currentlyTouchingA = CheckLineCollision(startPosition, "A");
                bool currentlyTouchingB = CheckLineCollision(startPosition, "B");

                // Inicia uma nova linha com a cor correta dependendo se está em contato com "A" ou "B" ou não
                StartNewLine((currentlyTouchingA || currentlyTouchingB) ? collisionLineColor : originalColor, startPosition);
                isTouchingA = currentlyTouchingA;
                isTouchingB = currentlyTouchingB;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentPosition.z = 0f;

                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], currentPosition) > 0.0001f) // Ajuste a distância mínima aqui
                {
                    bool currentlyTouchingA = CheckLineCollision(currentPosition, "A");
                    bool currentlyTouchingB = CheckLineCollision(currentPosition, "B");

                    if (currentlyTouchingA && !isTouchingA)
                    {
                        // Se a linha entra em contato com um objeto "A" e não estava tocando antes
                        isTouchingA = true;
                        StartNewLine(collisionLineColor, currentPosition);
                    }
                    else if (!currentlyTouchingA && isTouchingA)
                    {
                        // Se a linha sai de um contato com um objeto "A" e estava tocando antes
                        isTouchingA = false;
                        StartNewLine(originalColor, currentPosition);
                    }
                    else if (currentlyTouchingB && !isTouchingB)
                    {
                        // Se a linha entra em contato com um objeto "B" e não estava tocando antes
                        isTouchingB = true;
                        StartNewLine(collisionLineColor, currentPosition);
                    }
                    else if (!currentlyTouchingB && isTouchingB)
                    {
                        // Se a linha sai de um contato com um objeto "B" e estava tocando antes
                        isTouchingB = false;
                        StartNewLine(originalColor, currentPosition);
                    }
                    else
                    {
                        // Continua a linha atual
                        AddPointToLine(currentPosition);
                    }

                    // Acumula a área pintada dentro dos objetos "A" e "B"
                    if (currentlyTouchingA && points.Count > 1)
                    {
                        totalPaintedAreaA += Vector3.Distance(points[points.Count - 2], points[points.Count - 1]) * lineWidth;
                    }
                    if (currentlyTouchingB && points.Count > 1)
                    {
                        totalPaintedAreaB += Vector3.Distance(points[points.Count - 2], points[points.Count - 1]) * lineWidth;
                    }
                }
            }
        }

        // Verifica a porcentagem pintada em A
        if (letterColliderA != null)
        {
            float percentagePaintedA = (totalPaintedAreaA / CalculateLetterArea(letterColliderA)) * 100f;
            Debug.Log("Porcentagem Pintada A: " + percentagePaintedA.ToString("F2") + "%");

            if (percentagePaintedA >= requiredPercentage && letterObjectA.activeSelf)
            {
                Debug.Log("A pintada! Ocultando A e exibindo B.");
                letterObjectA.SetActive(false);
                letterObjectB.SetActive(true);
                ClearDrawing();
            }
        }

        // Verifica a porcentagem pintada em B
        if (letterColliderB != null)
        {
            float percentagePaintedB = (totalPaintedAreaB / CalculateLetterArea(letterColliderB)) * 100f;
            Debug.Log("Porcentagem Pintada B: " + percentagePaintedB.ToString("F2") + "%");

            if (percentagePaintedB >= requiredPercentage && letterObjectB.activeSelf)
            {
                Debug.Log("B completamente pintada!");
                letterObjectB.SetActive(false);
                ClearDrawing();
            }
        }
    }

    void StartNewLine(Color color, Vector3 startPosition)
    {
        if (currentLine != null)
        {
            // Finaliza a linha atual
            currentLine = null;
        }

        // Cria um novo GameObject e adiciona um LineRenderer a ele
        GameObject lineObject = new GameObject("Line");
        currentLine = lineObject.AddComponent<LineRenderer>(); // Adiciona o LineRenderer

        // Define a tag "Line"
        lineObject.tag = "Line"; // Define a tag após adicionar o LineRenderer

        // Configura o LineRenderer
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;

        // Define a cor da linha
        currentLine.startColor = color;
        currentLine.endColor = color;

        // Define o material como null para usar a cor padrão
        currentLine.material = new Material(Shader.Find("Sprites/Default"));

        // Renderiza a linha à frente dos objetos (definindo a ordem na camada de renderização)
        currentLine.sortingOrder = 1;

        points.Clear();

        // Adiciona o ponto inicial à nova linha
        points.Add(startPosition);
        currentLine.positionCount = points.Count;
        currentLine.SetPosition(points.Count - 1, startPosition);
    }

    void AddPointToLine(Vector3 position)
    {
        if (currentLine != null)
        {
            points.Add(position);
            currentLine.positionCount = points.Count;
            currentLine.SetPosition(points.Count - 1, position);
        }
    }

    void ToggleDrawing()
    {
        isDrawing = !isDrawing; // Inverte o estado de desenho

        // Se não estiver desenhando, limpa a linha atual
        if (!isDrawing && currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
    }

    bool CheckLineCollision(Vector3 position, string tag)
    {
        Collider2D collider = Physics2D.OverlapPoint(position);

        // Se a ponta da linha tocar um objeto com a tag especificada, retorna true
        if (collider != null && collider.CompareTag(tag))
        {
            return true;
        }

        return false;
    }

    float CalculateLetterArea(Collider2D collider)
    {
        if (collider == null) return 0f;

        // Obtém o tamanho do collider e calcula a área (aproximada para um retângulo)
        Bounds bounds = collider.bounds;
        return bounds.size.x * bounds.size.y;
    }

    void ClearDrawing()
    {
        // Destroi todos os objetos com a tag "Line"
        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }

        // Limpa a linha atual
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }

        // Limpa a lista de pontos
        points.Clear();
    }
}
