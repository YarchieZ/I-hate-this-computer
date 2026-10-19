using DG.Tweening;
using UnityEngine;

public class MonitorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private Material material;
    [SerializeField] private MonitorStatus monitorStatus;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float pressDistance = 0.1f;
    [SerializeField] private float duration = 0.1f;

    private Vector3 _initialPosition;
    private Tween _tween;

    private void Awake()
    {
        _initialPosition = transform.localPosition;
    }

    public void Interact()
    {
        monitorStatus.Turn();
        audioSource.Play();

        _tween?.Kill();
        transform.localPosition = _initialPosition;

        _tween = transform
            .DOLocalMoveX(_initialPosition.x + pressDistance, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
                _tween = transform
                    .DOLocalMoveX(_initialPosition.x, duration)
                    .SetEase(Ease.OutQuad));
    }
}