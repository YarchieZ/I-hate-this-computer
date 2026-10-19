using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ButtonSpawn : MonoBehaviour, IInteractable
{
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private SpawnOptions spawnOptions;
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private float pressDepth = 0.1f;
    private AudioSource audioSource;
    private Vector3 _initialPosition;
    private Tween _currentTween;
    
    private void Awake()
    {
        tipText.text = "Press ESC to open sensitivity settings";
        DestroyCoroutine(10f);
        audioSource = GetComponent<AudioSource>();
        if (buttonVisual == null)
            buttonVisual = transform;
        _initialPosition = buttonVisual.position;
    }

    public void Interact()
    {
        audioSource.Play();
        _currentTween?.Kill();
        _currentTween = DOTween.Sequence()
            .Append(buttonVisual.DOMove(_initialPosition + Vector3.down * pressDepth, 0.1f).SetEase(Ease.OutQuad))
            .Append(buttonVisual.DOMove(_initialPosition, 0.15f).SetEase(Ease.OutBack));

        if (spawnOptions == SpawnOptions.Keyboard)
        {
            if (objectSpawner.isKeyboardBroken)
            {
                objectSpawner.SpawnItem(SpawnOptions.Keyboard);
            }
            else
            {
                tipText.text = "Keyboard isnt fully broken";
                StartCoroutine(DestroyCoroutine(1f));
            }
        }
        else
        {
            if (objectSpawner.isMonitorBroken)
            {
                objectSpawner.SpawnItem(SpawnOptions.Monitor);
            }
            else
            {
                tipText.text = "Monitor isnt fully broken";
                StartCoroutine(DestroyCoroutine(1f));
            }
        }
    }
    private IEnumerator DestroyCoroutine(float value)
    {
        yield return new WaitForSeconds(value);
        tipText.text = "";
    }
}