using TMPro;
using UnityEngine;

public class MotnitorStatus : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float animationMaxSize;

    public void OnSpawn(GameManager gameManager)
    {
        textMesh.text = "";
        
    }

    public void LogError()
    {
        
    }

    public void UpdateUI(string text)
    {
        textMesh.text = text;
    }
}
