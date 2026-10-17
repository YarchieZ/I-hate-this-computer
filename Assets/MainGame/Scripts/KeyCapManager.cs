using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeyCapManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private KeyCap[] keycaps;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private TextMeshPro targetText;
    [SerializeField] private float rayDistance = 10f;
    private AudioSource audioSource;
    private KeyCap currentHoveredKeycap;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (var keycap in keycaps)
        {
            keycap.OnPressed += HandleKeycapPressed;
            keycap.OnPressedEarly += EarlyPressed;
        }
    }

    private void OnDisable()
    {
        foreach (var keycap in keycaps)
        {
            keycap.OnPressed -= HandleKeycapPressed;
            keycap.OnPressedEarly -= EarlyPressed;
        }
    }
    private void Update()
    {
        UpdateHover();
        CheckPressInput();
    }
    private void EarlyPressed()
    {
        int randomNumber = Random.Range(0, audioClips.Length);
        foreach (var audioClip in audioClips)
        {
            if (audioClip == audioClips[randomNumber])
            {
                audioSource.PlayOneShot(audioClip);
            }
            
        }
    }
    private void UpdateHover()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        KeyCap hitKeycap = null;

        foreach (var hit in hits)
        {
            KeyCap keycap = hit.collider.GetComponentInParent<KeyCap>();

            if (keycap != null)
            {
                hitKeycap = keycap;
                break; 
            }
        }

        currentHoveredKeycap = hitKeycap;
    }

    private void CheckPressInput()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (currentHoveredKeycap == null) return;

        currentHoveredKeycap.Press();
    }

    private void HandleKeycapPressed(KeyCap keycap)
    {
        targetText.text += keycap.Character;
    }
}