using TMPro;
using UnityEngine;

public class NotePadSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    public void OnSpawn(string sentence)
    {
        textMesh.text = sentence;
    }
    
    
}
