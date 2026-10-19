using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class KeyCapManager : MonoBehaviour
{
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private SpecialCaps specialCaps;
    [SerializeField] private KeyCap[] keycaps;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private string[] errorChars;
    private int durability;
    private Camera mainCamera;
    private MonitorStatus monitorStatus;
    private AudioSource audioSource;
    private KeyCap currentHoveredKeycap;
    private ObjectSpawner objectSpawnerMain;
    [SerializeField] private  BreakableObject breakableObject;
    private void Awake()
    {
        if (specialCaps == null)
            specialCaps = GetComponentInChildren<SpecialCaps>();
    }

    private void SendBreakInfo()
    {
        objectSpawnerMain.OnObjectBroken(SpawnOptions.Keyboard);
    }

    private void OnEnable()
    {
        durability = Random.Range(30, 40);
        if (breakableObject != null)
            breakableObject.OnBroken += SendBreakInfo;

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
        if (breakableObject != null)
            breakableObject.OnBroken -= SendBreakInfo;
    }

    private void Update()
    {
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

    public void UpdateInfo(MonitorStatus monitor, Camera camera, ObjectSpawner objectSpawner)
    {
        objectSpawnerMain = objectSpawner;
        mainCamera = camera;
        monitorStatus = monitor;
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
        if (durability > 15)
        {
            if (monitorStatus != null)
                monitorStatus.UpdateUI(keycap.Character, true);
            durability--;
        }
        else if (durability > 1)
        {
            
            int num = Random.Range(0, 120);
            if (num >= 40)
            {
                if (monitorStatus != null)
                    monitorStatus.UpdateUI(keycap.Character, true);
                durability--;
            }
            else if (num <= 10)
            {
                audioSource.PlayOneShot(errorClip);
                if (monitorStatus != null)
                    monitorStatus.UpdateUI(errorChars[Random.Range(0, errorChars.Length)], true);
            }
            else if (keycap.Character != null)
            {
                monitorStatus.LogError();
                audioSource.PlayOneShot(errorClip);
                string a = "";
                for (int i = 0; i < 3; i++)
                {
                    a += keycap.Character;
                }
                if (monitorStatus != null)
                    monitorStatus.UpdateUI(a, true);
            }
            else
            {
                monitorStatus.LogError();
                audioSource.PlayOneShot(errorClip);
            }
            
        }
        else if (durability == 1)
        {
            int num = Random.Range(0, 100);
            if (num >= 50)
            {
                audioSource.PlayOneShot(errorClip);
                if (monitorStatus != null)
                    monitorStatus.UpdateUI(errorChars[Random.Range(0, errorChars.Length)], true);
            }
            else if (num < 50 && keycap.Character != null)
            {
                monitorStatus.LogError();
                audioSource.PlayOneShot(errorClip);
                string a = "";
                for (int i = 0; i < 3; i++)
                {
                    a += keycap.Character;
                }
                if (monitorStatus != null)
                    monitorStatus.UpdateUI(a, true);
            }
            else
            {
                monitorStatus.LogError();
                audioSource.PlayOneShot(errorClip);
            }
        }
        
    }
}