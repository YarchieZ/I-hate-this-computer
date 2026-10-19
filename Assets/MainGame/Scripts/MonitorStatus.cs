using DG.Tweening;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class MonitorStatus : MonoBehaviour
{
    [SerializeField] private BreakableObject breakableObject;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float animationMaxSize;
    [SerializeField] private float scaleMultiplier = 1.4f;   
    [SerializeField] private float duration = 0.6f;      
    [SerializeField] private int vibrato = 20;               
    [SerializeField] private Color errorColor = Color.red;
    private ObjectSpawner objectSpawner;
    private Vector3 _baseScale;
    private Color _baseColor;
    private Vector3 _basePosition;
    private void OnEnable()
    {
        breakableObject.OnBroken += SendInfoToSpawner;
        _baseScale = textMesh.transform.localScale;
        _baseColor = textMesh.color;
        _basePosition = textMesh.transform.localPosition;
    }

    private void SendInfoToSpawner()
    {
        objectSpawner.OnObjectBroken(SpawnOptions.Monitor);
    }
    public string GetText()
    {
        return textMesh.text;
    }
    public void OnSpawn(ObjectSpawner gameManager)
    {
        textMesh.text = "";
        objectSpawner = gameManager;
    }

    public void CheckText()
    {
        if (textMesh.text == objectSpawner.currentNoteName)
        {
            textMesh.text = "";
            objectSpawner.SpawnItem(SpawnOptions.Note);
        }
        else
        {
            Debug.Log(objectSpawner.currentNoteName);
            Debug.Log(textMesh.text);
            LogError();
        }
    }
    private void LogError()
    {
        // Убиваем предыдущие твины, чтобы не накапливались
        textMesh.transform.DOKill();
        textMesh.DOKill();

        // Возвращаем к исходному состоянию на случай прерванного эффекта
        textMesh.transform.localScale = _baseScale;
        textMesh.color = _baseColor;

        Sequence seq = DOTween.Sequence();

        // 1. Увеличение + красный цвет
        seq.Append(textMesh.transform.DOScale(_baseScale * scaleMultiplier, duration * 0.3f)
            .SetEase(Ease.OutQuad));
        seq.Join(textMesh.DOColor(errorColor, duration * 0.3f));

        // 2. Небольшая пауза на пике
        seq.AppendInterval(duration * 0.15f);

        // 3. Возврат к исходному размеру и цвету
        seq.Append(textMesh.transform.DOScale(_baseScale, duration * 0.55f)
            .SetEase(Ease.OutBack));
        seq.Join(textMesh.DOColor(_baseColor, duration * 0.55f));

        seq.OnComplete(() =>
        {
            textMesh.transform.localScale = _baseScale;
            textMesh.color = _baseColor;
        });
    }

    public void UpdateUI(string text, bool show)
    {
        if (textMesh.text.Length > 63) return;
        if (show)
        {
            textMesh.text += text;
        }
        else
        {
            textMesh.text = text;
        }
    }

    private void OnDisable()
    {
        breakableObject.OnBroken -= SendInfoToSpawner;
    }
}
