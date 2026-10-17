using TMPro;
using UnityEngine;

public class NotePadSpawner : MonoBehaviour
{
    [SerializeField] private string[] sentences;
    [SerializeField] private TextMeshPro textMesh;
    public string CurrentSentence { get;  private set; }

    public void OnSpawn()
    {
        Debug.Log(CurrentSentence);
        CurrentSentence = sentences[Random.Range(0, sentences.Length)];
        textMesh.text = CurrentSentence;
    }
    
    
}
