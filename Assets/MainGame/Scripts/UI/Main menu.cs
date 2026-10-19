using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Mainmenu : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button authorsButton;
    [SerializeField] private Button settingsButton;

    [Header("Панели")]
    [SerializeField] private Image loadingPanel;
    [SerializeField] private RectTransform authorsPanel;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Логотип")]
    [SerializeField] private RectTransform logo;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip playClickSound;

    [Header("Настройки анимации")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float popupDuration = 0.35f;
    [SerializeField] private float popupStartScale = 0.7f;
    [SerializeField] private float logoShrinkScale = 0.5f;
    [SerializeField] private float logoMoveDuration = 0.4f;
    [SerializeField] private Vector2 logoCornerOffset = new Vector2(40f, -40f);
    [SerializeField] private string sceneName = "SampleScene";

    private bool isLoading = false;

    private Vector3 logoOriginalScale;
    private Vector2 logoOriginalPosition;
    private bool logoCaptured = false;

    private bool authorsOpen = false;
    private bool settingsOpen = false;

    private void Awake()
    {
        if (loadingPanel != null)
        {
            Color c = loadingPanel.color;
            c.a = 0f;
            loadingPanel.color = c;
            loadingPanel.raycastTarget = false;
            loadingPanel.gameObject.SetActive(true);
        }

        HidePanelImmediate(authorsPanel);
        HidePanelImmediate(settingsPanel);
        authorsOpen = false;
        settingsOpen = false;

        if (logo != null)
        {
            logoOriginalScale = logo.localScale;
            logoOriginalPosition = logo.anchoredPosition;
            logoCaptured = true;
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (authorsButton != null)
            authorsButton.onClick.AddListener(OnAuthorsClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);

        if (authorsButton != null)
            authorsButton.onClick.RemoveListener(OnAuthorsClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        if (isLoading) return;
        isLoading = true;

        PlayClickSound(playClickSound);
        AnimatePress(playButton.transform, ShowLoadingAndLoadScene);
    }

    private void ShowLoadingAndLoadScene()
    {
        if (loadingPanel == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        loadingPanel.raycastTarget = true;

        loadingPanel.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    SceneManager.LoadScene(sceneName);
                });
            });
    }

    private void OnQuitClicked()
    {
        PlayClickSound(clickSound);
        AnimatePress(quitButton.transform, () =>
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        });
    }

    private void OnAuthorsClicked()
    {
        PlayClickSound(clickSound);
        AnimatePress(authorsButton.transform, () => TogglePanel(authorsPanel));
    }

    private void OnSettingsClicked()
    {
        PlayClickSound(clickSound);
        AnimatePress(settingsButton.transform, () => TogglePanel(settingsPanel));
    }

    private void TogglePanel(RectTransform panel)
    {
        if (panel == null) return;

        bool isOpen = (panel == authorsPanel && authorsOpen) || (panel == settingsPanel && settingsOpen);

        if (isOpen)
        {
            HidePopup(panel);
            if (panel == authorsPanel) authorsOpen = false;
            if (panel == settingsPanel) settingsOpen = false;
        }
        else
        {
            ShowPopup(panel);
            if (panel == authorsPanel) authorsOpen = true;
            if (panel == settingsPanel) settingsOpen = true;
        }

        UpdateLogoState();
    }

    private void ShowPopup(RectTransform panel)
    {
        if (panel == null) return;

        panel.DOKill();
        panel.gameObject.SetActive(true);
        panel.localScale = Vector3.one * popupStartScale;

        Image bg = panel.GetComponent<Image>();
        if (bg != null)
        {
            Color c = bg.color;
            c.a = 0f;
            bg.color = c;
            bg.raycastTarget = true;
            bg.DOFade(1f, popupDuration).SetEase(Ease.OutQuad);
        }

        panel.DOScale(Vector3.one, popupDuration)
            .SetEase(Ease.OutBack);
    }

    private void HidePopup(RectTransform panel)
    {
        if (panel == null) return;

        panel.DOKill();

        Image bg = panel.GetComponent<Image>();
        Sequence seq = DOTween.Sequence();

        if (bg != null)
        {
            bg.raycastTarget = false;
            seq.Join(bg.DOFade(0f, popupDuration * 0.8f).SetEase(Ease.InQuad));
        }

        seq.Join(panel.DOScale(Vector3.one * popupStartScale, popupDuration * 0.8f).SetEase(Ease.InQuad));
        seq.OnComplete(() => panel.gameObject.SetActive(false));
    }

    private void HidePanelImmediate(RectTransform panel)
    {
        if (panel == null) return;

        panel.localScale = Vector3.one * popupStartScale;

        Image bg = panel.GetComponent<Image>();
        if (bg != null)
        {
            Color c = bg.color;
            c.a = 0f;
            bg.color = c;
            bg.raycastTarget = false;
        }

        panel.gameObject.SetActive(false);
    }

    private void UpdateLogoState()
    {
        if (logo == null || !logoCaptured) return;

        bool anyPanelOpen = authorsOpen || settingsOpen;

        logo.DOKill();

        if (anyPanelOpen)
        {
            Vector2 targetPos = GetLogoCornerPosition();

            Sequence seq = DOTween.Sequence();
            seq.Join(logo.DOAnchorPos(targetPos, logoMoveDuration).SetEase(Ease.OutCubic));
            seq.Join(logo.DOScale(logoOriginalScale * logoShrinkScale, logoMoveDuration).SetEase(Ease.OutCubic));
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            seq.Join(logo.DOAnchorPos(logoOriginalPosition, logoMoveDuration).SetEase(Ease.OutCubic));
            seq.Join(logo.DOScale(logoOriginalScale, logoMoveDuration).SetEase(Ease.OutCubic));
        }
    }

    private Vector2 GetLogoCornerPosition()
    {
        RectTransform parent = logo.parent as RectTransform;
        if (parent == null) return logoOriginalPosition;

        float parentHalfWidth = parent.rect.width * 0.5f;
        float parentHalfHeight = parent.rect.height * 0.5f;

        Vector3 targetScale = logoOriginalScale * logoShrinkScale;
        float scaledWidth = logo.rect.width * targetScale.x;
        float scaledHeight = logo.rect.height * targetScale.y;

        float pivotX = logo.pivot.x;
        float pivotY = logo.pivot.y;

        float leftEdgeOfCanvas = -parentHalfWidth;
        float topEdgeOfCanvas = parentHalfHeight;

        float targetX = leftEdgeOfCanvas + logoCornerOffset.x + scaledWidth * pivotX;
        float targetY = topEdgeOfCanvas + logoCornerOffset.y - scaledHeight * (1f - pivotY);

        return new Vector2(targetX, targetY);
    }

    private void PlayClickSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void AnimatePress(Transform buttonTransform, System.Action onComplete)
    {
        if (buttonTransform == null)
        {
            onComplete?.Invoke();
            return;
        }

        buttonTransform.DOKill();

        Vector3 originalScale = Vector3.one;

        buttonTransform.DOScale(originalScale * pressScale, pressDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                buttonTransform.DOScale(originalScale, pressDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
            });
    }
}