using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LoseMenu : MonoBehaviour
{
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private RectTransform losePanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private float popupDuration = 0.35f;
    [SerializeField] private float popupStartScale = 0.7f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isOpen = false;

    private void Awake()
    {

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

    public void ShowLose()
    {
        if (losePanel == null) return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        losePanel.gameObject.SetActive(true);
        mouseLook.enabled = false;
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