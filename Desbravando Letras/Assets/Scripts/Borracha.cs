using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Borracha : MonoBehaviour
{
    public Button botaoApagarLinhas; // Botão para ativar/desativar o desenho

    void Start()
    {
        // Adiciona um listener ao botão para chamar a função ApagarLinhas quando clicado
        botaoApagarLinhas.onClick.AddListener(ApagarLinhas);
    }

    void ApagarLinhas()
    {
        // Encontra todos os objetos com a tag "line" e os destroi
        GameObject[] linhas = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject linha in linhas)
        {
            Destroy(linha);
        }
    }
}
