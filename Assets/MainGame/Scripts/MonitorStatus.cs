using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class MonitorStatus : MonoBehaviour
{
    [SerializeField] private GameObject display;
    [SerializeField] private Material redIndicatorMat;
    [SerializeField] private Material greenIndicatorMat;
    [SerializeField] private GameObject indicator;
    [SerializeField] private Material turnOnMaterial;
    [SerializeField] private Material turnOffMaterial;
    [SerializeField] private BreakableObject breakableObject;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float animationMaxSize;
    [SerializeField] private float scaleMultiplier = 1.4f;   
    [SerializeField] private float duration = 0.6f;      
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private AudioClip correctAudioClip;
    [SerializeField] private AudioClip failAudioClip;
    [SerializeField] private AudioClip turnOffAudioClip;
    [SerializeField] private AudioClip turnOnAudioClip;
    private int durability;
    [SerializeField] VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private ObjectSpawner objectSpawner;
    private Vector3 _baseScale;
    private Color _baseColor;
    private Vector3 _basePosition;
    private bool turnOn;

    private string _baseText = "";
    private bool _cursorVisible;
    private float _cursorTimer;
    private const float CursorBlinkInterval = 0.5f;
    private const string CursorChar = "_";

    private void OnEnable()
    {
        durability = Random.Range(80, 110);
        turnOn = true;
        audioSource = GetComponent<AudioSource>();
        breakableObject.OnBroken += SendInfoToSpawner;
        _baseScale = textMesh.transform.localScale;
        _baseColor = textMesh.color;
        _basePosition = textMesh.transform.localPosition;
        _baseText = "";
        _cursorVisible = false;
        _cursorTimer = 0f;
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
        _baseText = "";
        objectSpawner = gameManager;
    }

    public void CheckText()
    {
        if (_baseText == objectSpawner.currentNoteName)
        {
            audioSource.PlayOneShot(correctAudioClip);
            textMesh.text = "";
            _baseText = "";
            objectSpawner.SpawnItem(SpawnOptions.Note);
        }
        else
        {
            Debug.Log(objectSpawner.currentNoteName);
            Debug.Log(_baseText);
            LogError();
        }
    }
    public void LogError()
    {
        audioSource.PlayOneShot(failAudioClip);
        textMesh.transform.DOKill();
        textMesh.DOKill();
        textMesh.transform.localScale = _baseScale;
        textMesh.color = _baseColor;
        Sequence seq = DOTween.Sequence();
        seq.Append(textMesh.transform.DOScale(_baseScale * scaleMultiplier, duration * 0.3f)
            .SetEase(Ease.OutQuad));
        seq.Join(textMesh.DOColor(errorColor, duration * 0.3f));
        seq.AppendInterval(duration * 0.15f);
        seq.Append(textMesh.transform.DOScale(_baseScale, duration * 0.55f)
            .SetEase(Ease.OutBack));
        seq.Join(textMesh.DOColor(_baseColor, duration * 0.55f));

        seq.OnComplete(() =>
        {
            textMesh.transform.localScale = _baseScale;
            textMesh.color = _baseColor;
        });
    }

    public void Turn()
    {
        if (durability == 1) 
        {
            Renderer objectRenderer = display.GetComponent<Renderer>();
            objectRenderer.material = turnOnMaterial;
            textMesh.transform.DOKill();
            textMesh.gameObject.SetActive(true);
            audioSource.PlayOneShot(turnOnAudioClip);
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            indicatorRenderer.material = greenIndicatorMat;
            return;
        }
        if (turnOn)
        {
            Renderer objectRenderer = display.GetComponent<Renderer>();
            objectRenderer.material = turnOffMaterial;
            textMesh.transform.DOKill();
            textMesh.gameObject.SetActive(false);
            audioSource.PlayOneShot(turnOffAudioClip);
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            indicatorRenderer.material = redIndicatorMat;
            videoPlayer.enabled = false;
        }
        else
        {
            Renderer objectRenderer = display.GetComponent<Renderer>();
            objectRenderer.material = turnOnMaterial;
            textMesh.transform.DOKill();
            textMesh.gameObject.SetActive(true);
            audioSource.PlayOneShot(turnOnAudioClip);
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            indicatorRenderer.material = greenIndicatorMat;
            videoPlayer.enabled = true;
        }
        turnOn = !turnOn;
    }

    public void UpdateUI(string text, bool show)
    {
        Debug.Log(durability);
        if (turnOn)
        {
            
            if (durability >= 40)
            {
                if (_baseText.Length > 63) return;
                if (show)
                {
                    _baseText += text;
                }
                else
                {
                    _baseText = text;
                }
                RefreshDisplayText();
                durability--;
            }
            else if (durability > 1)
            {
                int num =  Random.Range(0, 100);
                if (num >= 10)
                {
                    if (_baseText.Length > 63) return;
                    if (show)
                    {
                        _baseText += text;
                    }
                    else
                    {
                        _baseText = text;
                    }
                    RefreshDisplayText();
                    durability--;
                }
                else
                {
                    if (turnOn)
                    {
                        Turn();
                    }
                }
            }
            else
            {
                if (turnOn)
                {
                    Turn();
                }
                
            }
        }

    }

    private void Update()
    {
        if (!turnOn || textMesh == null || !textMesh.gameObject.activeSelf) return;

        _cursorTimer += Time.deltaTime;
        if (_cursorTimer >= CursorBlinkInterval)
        {
            _cursorTimer -= CursorBlinkInterval;
            _cursorVisible = !_cursorVisible;
            RefreshDisplayText();
        }
    }

    private void RefreshDisplayText()
    {
        textMesh.text = _cursorVisible ? _baseText + CursorChar : _baseText;
    }

    private void OnDisable()
    {
        breakableObject.OnBroken -= SendInfoToSpawner;
    }
}