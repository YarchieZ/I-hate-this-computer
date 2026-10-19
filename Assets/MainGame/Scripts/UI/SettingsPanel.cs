using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class SettingsPanel : MonoBehaviour
{
    [Header("Панель настроек")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Компоненты, отключаемые при паузе")]
    [SerializeField] private List<MonoBehaviour> componentsToDisable = new List<MonoBehaviour>();

    [Header("Настройки звука")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();

    [Header("Чувствительность мыши")]
    [Tooltip("Скрипт MouseLook, чью чувствительность будем менять")]
    [SerializeField] private MouseLook mouseLook;
    [Tooltip("TMP Input Field для значения чувствительности (1..100)")]
    [SerializeField] private TMP_InputField sensitivityInput;

    [Header("Курсор")]
    [SerializeField] private bool showCursorOnPause = true;

    private const float MIN_SENSITIVITY = 1f;
    private const float MAX_SENSITIVITY = 100f;
    private const float DEFAULT_SENSITIVITY = 10f;

    private InputAction escAction;
    private bool isPaused = false;
    private float previousTimeScale = 1f;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // --- Громкость ---
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            OnVolumeChanged(volumeSlider.value);
            volumeSlider.value = 0.5f;
        }

        // --- Чувствительность ---
        if (mouseLook != null)
        {
            // Если в MouseLook значение по умолчанию не выставлено — ставим 50
            if (mouseLook.SensitivityValue <= 0f)
                mouseLook.SensitivityValue = DEFAULT_SENSITIVITY;
        }

        if (sensitivityInput != null)
        {
            // Только целые числа (1..100) — можно сделать и Decimal, если нужны дробные
            sensitivityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            sensitivityInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
            sensitivityInput.onEndEdit.AddListener(OnSensitivityInputEndEdit);
            sensitivityInput.onSubmit.AddListener(OnSensitivityInputSubmit);
            sensitivityInput.onValueChanged.AddListener(OnSensitivityInputValueChanged);
            RefreshSensitivityField();
        }

        // --- ESC ---
        escAction = new InputAction("ToggleSettings", InputActionType.Button, "<Keyboard>/escape");
        escAction.performed += OnEscPerformed;
    }

    private void OnEnable()
    {
        escAction?.Enable();
    }

    private void OnDisable()
    {
        escAction?.Disable();
    }

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

    // ==================== ЧУВСТВИТЕЛЬНОСТЬ ====================

    /// <summary>
    /// Жёсткая фильтрация ввода: только цифры (без точки/запятой/минуса).
    /// </summary>
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

    private void OnSensitivityInputEndEdit(string text)
    {
        ApplySensitivityFromText(text);
    }

    private void OnSensitivityInputSubmit(string text)
    {
        ApplySensitivityFromText(text);
    }

    private void ApplySensitivityFromText(string text)
    {
        if (mouseLook == null) return;

        if (int.TryParse(text, System.Globalization.NumberStyles.Integer,
                         System.Globalization.CultureInfo.InvariantCulture, out int value))
        {
            float clamped = Mathf.Clamp(value, MIN_SENSITIVITY, MAX_SENSITIVITY);
            mouseLook.SensitivityValue = clamped;
        }
        else
        {
            // Если ввели пусто/невалидно — вернуть текущее значение
        }

        RefreshSensitivityField();
    }

    // ==================== ГРОМКОСТЬ ====================

    private void OnVolumeChanged(float value)
    {
        foreach (var source in audioSources)
        {
            if (source != null)
                source.volume = value;
        }
    }

    // ==================== ESC / ПАУЗА ====================

    private void OnEscPerformed(InputAction.CallbackContext ctx)
    {
        TogglePanel();
    }

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