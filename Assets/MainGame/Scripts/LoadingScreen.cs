using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private Tween _fadeTween;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = loadingPanel.AddComponent<CanvasGroup>();
    }

    public void ShowLoadingPanel()
    {
        FadeTo(1f, true);
    }

    public void HideLoadingPanel()
    {
        FadeTo(0f, false);
    }

    private void FadeTo(float targetAlpha, bool blockInput)
    {
        if (_canvasGroup == null) return;

        _fadeTween?.Kill();
        _fadeTween = _canvasGroup
            .DOFade(targetAlpha, fadeDuration)
            .SetEase(fadeEase)
            .OnStart(() =>
            {
                _canvasGroup.blocksRaycasts = blockInput;
                _canvasGroup.interactable = blockInput;
            })
            .OnComplete(() =>
            {
                if (!blockInput)
                {
                    _canvasGroup.blocksRaycasts = false;
                    _canvasGroup.interactable = false;
                }
            });
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }
}