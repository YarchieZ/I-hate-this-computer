using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeyCapManager : MonoBehaviour
{
    [SerializeField] private SpecialCaps specialCaps;
    [SerializeField] private KeyCap[] keycaps;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float rayDistance = 10f;
    private Camera mainCamera;
    private MonitorStatus monitorStatus;
    private AudioSource audioSource;
    private KeyCap currentHoveredKeycap;

    // Пытаемся найти SpecialCaps, если он не назначен в инспекторе
    private void Awake()
    {
        if (specialCaps == null)
            specialCaps = GetComponentInChildren<SpecialCaps>();
    }

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
        // Защита от null, пока камера не передана
        if (mainCamera == null) return; 

        UpdateHover();
        CheckPressInput();
    }

    private void EarlyPressed()
    {
        if (audioClips == null || audioClips.Length == 0 || audioSource == null) return;
        int randomNumber = Random.Range(0, audioClips.Length);
        audioSource.PlayOneShot(audioClips[randomNumber]);
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

    public void UpdateInfo(MonitorStatus monitor, Camera camera)
    {
        mainCamera = camera;
        monitorStatus = monitor;
        
        // Передаем монитор в SpecialCaps, если он найден
        if (specialCaps != null && monitorStatus != null)
        {
            specialCaps.UpdateInfo(monitorStatus);
        }
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
        if (monitorStatus != null)
            monitorStatus.UpdateUI(keycap.Character, true);
    }
}