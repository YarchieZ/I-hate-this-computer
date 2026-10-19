using TMPro;
using UnityEngine;

public class NotePadSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    public bool isMeCurrent { get; private set; }

    public void ChangeCurrentNote()
    {
        isMeCurrent = !isMeCurrent;
    }
    public void OnSpawn(string sentence)
    {
        ChangeCurrentNote();
        textMesh.text = sentence;
    }
    
    
}
