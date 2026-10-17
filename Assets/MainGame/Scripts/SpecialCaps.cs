using TMPro;
using UnityEngine;

public class SpecialCaps : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    public void OnBackspace()
    {
        string text = textMesh.text.Substring(0, textMesh.text.Length - 2);
        textMesh.text = text;
    }
}
