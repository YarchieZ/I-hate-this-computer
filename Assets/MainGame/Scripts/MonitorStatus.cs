using TMPro;
using UnityEngine;

public class MonitorStatus : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float animationMaxSize;

    public string GetText()
    {
        return textMesh.text;
    }
    public void OnSpawn(ObjectSpawner gameManager)
    {
        textMesh.text = "";
        
    }

    public void LogError()
    {
        
    }

    public void UpdateUI(string text, bool show)
    {
        if (show)
        {
            textMesh.text += text;
        }
        else
        {
            textMesh.text = text;
        }
    }
}
