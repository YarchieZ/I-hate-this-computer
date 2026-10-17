using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class KeyCap : MonoBehaviour
{
    [SerializeField] private string character;  
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float pressDistance = 0.01f;
    [SerializeField] private UnityEvent unityEvent;
    public string Character => character;
    public bool IsAnimating { get; private set; }
    public event Action<KeyCap> OnPressed;
    public event Action OnPressedEarly;
    private Vector3 originalPosition;
    private void Awake()
    {
        originalPosition = transform.localPosition;
    }
    public void Press()
    {
        if (IsAnimating) return;
        PlayPressAnimation();
        
    }

    private void PlayPressAnimation()
    {
        OnPressedEarly?.Invoke();
        IsAnimating = true;
        Vector3 targetPos = originalPosition + Vector3.down * pressDistance;
        transform.DOLocalMove(targetPos, animationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOLocalMove(originalPosition, animationDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        IsAnimating = false;
                        OnPressed?.Invoke(this);
                        if (unityEvent != null) unityEvent.Invoke();
                    });
            });
    }
}