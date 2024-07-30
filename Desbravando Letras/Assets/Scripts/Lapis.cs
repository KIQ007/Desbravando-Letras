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

    private Collider2D letterColliderA; // Collider da letra A
    private GameObject letterObjectA; // Objeto da letra A
    private GameObject letterObjectB; // Objeto da letra B

    [SerializeField]
    private float requiredPercentage = 60f; // Porcentagem requerida de pintura

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
                bool currentlyTouchingA = CheckLineCollision(startPosition);

                // Inicia uma nova linha com a cor correta dependendo se está em contato com "A" ou não
                StartNewLine(currentlyTouchingA ? collisionLineColor : originalColor, startPosition);
                isTouchingA = currentlyTouchingA;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentPosition.z = 0f;

                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], currentPosition) > 0.001f)
                {
                    bool currentlyTouchingA = CheckLineCollision(currentPosition);

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
                    else
                    {
                        // Continua a linha atual
                        AddPointToLine(currentPosition);
                    }
                }

                if (letterColliderA != null)
                {
                    // Calcula a área pintada até o momento dentro do objeto "A"
                    float paintedArea = CalculatePaintedAreaInsideA();

                    // Calcula a porcentagem pintada em relação à área total da letra
                    float percentagePainted = (paintedArea / CalculateLetterArea(letterColliderA)) * 100f;

                    // Exemplo de uso: imprime a porcentagem pintada no console
                    Debug.Log("Porcentagem Pintada: " + percentagePainted.ToString("F2") + "%");

                    // Se a porcentagem pintada for igual ou superior à porcentagem requerida, oculta o objeto A e mostra o objeto B
                    if (percentagePainted >= requiredPercentage)
                    {
                        letterObjectA.SetActive(false);
                        letterObjectB.SetActive(true);
                    }
                }
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
        points.Add(position);
        currentLine.positionCount = points.Count;
        currentLine.SetPosition(points.Count - 1, position);
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

    bool CheckLineCollision(Vector3 position)
    {
        Collider2D collider = Physics2D.OverlapPoint(position);

        // Se a ponta da linha tocar um objeto com a tag "A", retorna true
        if (collider != null && collider.CompareTag("A"))
        {
            return true;
        }

        return false;
    }

    float CalculatePaintedAreaInsideA()
    {
        if (letterColliderA == null)
        {
            return 0f;
        }

        float paintedArea = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 midPoint = (points[i] + points[i - 1]) / 2;

            if (letterColliderA.OverlapPoint(midPoint))
            {
                paintedArea += Vector3.Distance(points[i], points[i - 1]) * lineWidth;
            }
        }

        return paintedArea;
    }

    float CalculateLetterArea(Collider2D collider)
    {
        // Obtém o tamanho da caixa delimitadora do collider da letra
        Bounds bounds = collider.bounds;
        return bounds.size.x * bounds.size.y; // Área = largura * altura
    }

    // Método público para definir a porcentagem requerida de pintura
    public void SetRequiredPercentage(float percentage)
    {
        requiredPercentage = percentage;
    }
}
