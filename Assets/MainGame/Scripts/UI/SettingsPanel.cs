using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private List<MonoBehaviour> componentsToDisable = new List<MonoBehaviour>();
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private TMP_InputField sensitivityInput;
    [SerializeField] private bool showCursorOnPause = true;

    private const float MIN_SENSITIVITY = 1f;
    private const float MAX_SENSITIVITY = 100f;
    private const float DEFAULT_SENSITIVITY = 10f;
    private const float DEFAULT_VOLUME = 0.5f;

    private const string PREFS_VOLUME = "Settings.Volume";
    private const string PREFS_SENSITIVITY = "Settings.Sensitivity";

    private InputAction escAction;
    private bool isPaused = false;
    private float previousTimeScale = 1f;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        LoadSettings(out float savedVolume, out float savedSensitivity);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.SetValueWithoutNotify(savedVolume);
            OnVolumeChanged(savedVolume);
        }

        if (mouseLook != null)
        {
            mouseLook.SensitivityValue = savedSensitivity;
        }

        if (sensitivityInput != null)
        {
            sensitivityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            sensitivityInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
            sensitivityInput.onEndEdit.AddListener(OnSensitivityInputEndEdit);
            sensitivityInput.onSubmit.AddListener(OnSensitivityInputSubmit);
            sensitivityInput.onValueChanged.AddListener(OnSensitivityInputValueChanged);
            RefreshSensitivityField();
        }

        escAction = new InputAction("ToggleSettings", InputActionType.Button, "<Keyboard>/escape");
        escAction.performed += OnEscPerformed;
    }

    private static void LoadSettings(out float volume, out float sensitivity)
    {
        volume = PlayerPrefs.GetFloat(PREFS_VOLUME, DEFAULT_VOLUME);
        sensitivity = PlayerPrefs.GetFloat(PREFS_SENSITIVITY, DEFAULT_SENSITIVITY);

        volume = Mathf.Clamp01(volume);
        if (sensitivity <= 0f)
            sensitivity = DEFAULT_SENSITIVITY;
        sensitivity = Mathf.Clamp(sensitivity, MIN_SENSITIVITY, MAX_SENSITIVITY);
    }

    private void SaveVolume(float value)
    {
        PlayerPrefs.SetFloat(PREFS_VOLUME, value);
        PlayerPrefs.Save();
    }

    private void SaveSensitivity(float value)
    {
        PlayerPrefs.SetFloat(PREFS_SENSITIVITY, value);
        PlayerPrefs.Save();
    }

    private void OnSensitivityInputValueChanged(string text)
    {
        if (sensitivityInput == null) return;

        string filtered = FilterIntegerText(text);
        if (filtered != text)
        {
            sensitivityInput.SetTextWithoutNotify(filtered);
            sensitivityInput.caretPosition = filtered.Length;
            sensitivityInput.selectionAnchorPosition = filtered.Length;
            sensitivityInput.selectionFocusPosition = filtered.Length;
        }
    }

    private static string FilterIntegerText(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        StringBuilder sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (c >= '0' && c <= '9')
                sb.Append(c);
        }
        return sb.ToString();
    }

    private void RefreshSensitivityField()
    {
        if (mouseLook == null || sensitivityInput == null) return;
        sensitivityInput.SetTextWithoutNotify(
            Mathf.RoundToInt(mouseLook.SensitivityValue).ToString());
    }

    private void OnSensitivityInputEndEdit(string text) => ApplySensitivityFromText(text);
    private void OnSensitivityInputSubmit(string text) => ApplySensitivityFromText(text);

    private void ApplySensitivityFromText(string text)
    {
        if (mouseLook == null) return;

        if (int.TryParse(text, System.Globalization.NumberStyles.Integer,
                         System.Globalization.CultureInfo.InvariantCulture, out int value))
        {
            float clamped = Mathf.Clamp(value, MIN_SENSITIVITY, MAX_SENSITIVITY);
            mouseLook.SensitivityValue = clamped;
            SaveSensitivity(clamped);
        }
        RefreshSensitivityField();
    }

    private void OnVolumeChanged(float value)
    {
        foreach (var source in audioSources)
        {
            if (source != null)
                source.volume = value;
        }
        SaveVolume(value);
    }

    private void OnEnable()  => escAction?.Enable();
    private void OnDisable() => escAction?.Disable();

    private void OnDestroy()
    {
        if (escAction != null)
        {
            escAction.performed -= OnEscPerformed;
            escAction.Dispose();
            escAction = null;
        }

        if (isPaused)
        {
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            RestoreCursor();
        }

        if (mouseLook != null)
            mouseLook.IsInputBlocked = false;

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (sensitivityInput != null)
        {
            sensitivityInput.onEndEdit.RemoveListener(OnSensitivityInputEndEdit);
            sensitivityInput.onSubmit.RemoveListener(OnSensitivityInputSubmit);
            sensitivityInput.onValueChanged.RemoveListener(OnSensitivityInputValueChanged);
        }
    }

    private void OnEscPerformed(InputAction.CallbackContext ctx) => TogglePanel();

    private void TogglePanel()
    {
        isPaused = !isPaused;

        if (settingsPanel != null)
            settingsPanel.SetActive(isPaused);

        SetPause(isPaused);
    }

    private void SetPause(bool pause)
    {
        if (pause)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            foreach (var comp in componentsToDisable)
            {
                if (comp != null)
                    comp.enabled = false;
            }

            if (mouseLook != null)
                mouseLook.IsInputBlocked = true;

            if (showCursorOnPause)
                ApplyPauseCursor();

            RefreshSensitivityField();
        }
        else
        {
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

            foreach (var comp in componentsToDisable)
            {
                if (comp != null)
                    comp.enabled = true;
            }

            if (mouseLook != null)
                mouseLook.IsInputBlocked = false;

            if (showCursorOnPause)
                RestoreCursor();
        }
    }

    private void ApplyPauseCursor()
    {
        previousLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    private void RestoreCursor()
    {
        Cursor.lockState = previousLockMode;
        Cursor.visible = previousCursorVisible;
    }
}