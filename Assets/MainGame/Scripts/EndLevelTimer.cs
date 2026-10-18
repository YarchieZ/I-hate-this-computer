using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelTimer : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Slider slider;
    private float _currentTime;
    private bool _isX2;
    private bool isShaking;
    private RectTransform originalPosition;

    private void Start()
    {
        originalPosition = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (_isX2 && _currentTime < duration)
        {
            _currentTime += Time.deltaTime;
            slider.value = 1 - (_currentTime / (duration / 2));
        }
        else if (!_isX2 && _currentTime < duration)
        {
            _currentTime += Time.deltaTime;
            slider.value = 1 - (_currentTime / duration);
        }
        else
        {
            
        }

        if (!isShaking && _currentTime > (duration / 100) * 80)
        {
            isShaking = true;
            originalPosition.DOShakeAnchorPos(
                duration: 0.3f, 
                strength: new Vector2(10f, 5f), 
                vibrato: 15, 
                randomness: 90, 
                snapping: false, 
                fadeOut: true
            ).OnComplete(() =>
                isShaking = false);
        }
    }
}
