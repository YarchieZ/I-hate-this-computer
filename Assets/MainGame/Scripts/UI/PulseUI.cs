using DG.Tweening;
using UnityEngine;

public class PulseUI : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Частота пульсации (циклов в секунду)")]
    [SerializeField] private float frequency = 1f;

    [Tooltip("Интенсивность (на сколько увеличивается размер, 0.1 = +10%)")]
    [SerializeField] private float intensity = 0.1f;

    private Vector3 _baseScale;
    private Tween _pulseTween;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        _pulseTween?.Kill();
        _pulseTween = null;
        transform.localScale = _baseScale;
    }

    private void StartPulse()
    {
        _pulseTween?.Kill();

        float duration = 1f / Mathf.Max(0.01f, frequency);
        Vector3 targetScale = _baseScale * (1f + intensity);

        transform.localScale = _baseScale;

        _pulseTween = transform
            .DOScale(targetScale, duration * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            StartPulse();
        }
    }
}