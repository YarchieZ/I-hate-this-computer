using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [Header("Панель паузы")]
    [SerializeField] private RectTransform pausePanel;

    [Header("Кнопка возврата в главное меню")]
    [SerializeField] private Button mainMenuButton;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [Header("Настройки анимации")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private float popupDuration = 0.35f;
    [SerializeField] private float popupStartScale = 0.7f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isOpen = false;

    private void Awake()
    {
        HidePanelImmediate(pausePanel);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    public void ShowPause()
    {
        if (pausePanel == null) return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowPopup(pausePanel);
        isOpen = true;
    }

    private void OnMainMenuClicked()
    {
        PlayClickSound();
        AnimatePress(mainMenuButton.transform, () =>
        {
            SceneManager.LoadScene(mainMenuSceneName);
        });
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

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
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