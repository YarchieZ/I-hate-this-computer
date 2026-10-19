using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelTimer : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Slider slider;
    private float _currentTime;
    private bool isShaking;
    private RectTransform originalPosition;

    private void Start()
    {
        originalPosition = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (_currentTime < duration)
        {
            _currentTime += Time.deltaTime;
            slider.value = 1 - (_currentTime / duration);
        }
        else
        {
            
        }
    }
}
