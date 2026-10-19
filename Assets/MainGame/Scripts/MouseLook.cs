using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Чувствительность")]
    [SerializeField, Range(1f, 100f)] private float sensitivityValue = 50f;
    [SerializeField] private float maxSensitivityMultiplier = 0.6f;

    [Header("Ограничения по вертикали")]
    [SerializeField] private float minY = -80f;
    [SerializeField] private float maxY = 80f;

    [Header("Курсор")]
    [SerializeField] private bool lockCursor = true;

    private float xRotation;
    private float yRotation;
    private float cachedMultiplier;      // пересчитывается только при смене чувствительности

    private Transform yawPivot;
    private Transform pitchPivot;

    private static readonly Vector3 AxisX = Vector3.right;
    private static readonly Vector3 AxisY = Vector3.up;

    public bool IsInputBlocked { get; set; }

    public float SensitivityValue
    {
        get => sensitivityValue;
        set
        {
            sensitivityValue = Mathf.Clamp(value, 1f, 100f);
            cachedMultiplier = (sensitivityValue / 100f) * maxSensitivityMultiplier;
        }
    }

    private void Awake()
    {
        pitchPivot = transform;
        yawPivot   = transform.parent;

        xRotation = NormalizeAngle(pitchPivot.localEulerAngles.x);
        yRotation = yawPivot != null ? NormalizeAngle(yawPivot.localEulerAngles.y) : 0f;

        // Применяем один раз при старте
        pitchPivot.localRotation = Quaternion.AngleAxis(xRotation, AxisX);
        if (yawPivot != null)
            yawPivot.localRotation = Quaternion.AngleAxis(yRotation, AxisY);

        // Инициализация кэша (не через свойство, чтобы не триггерить лишний Clamp)
        cachedMultiplier = (sensitivityValue / 100f) * maxSensitivityMultiplier;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        cachedMultiplier = (sensitivityValue / 100f) * maxSensitivityMultiplier;
    }
#endif

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (IsInputBlocked) return;

        var mouse = Mouse.current;
        if (mouse == null || yawPivot == null) return;

        Vector2 delta = mouse.delta.ReadValue() * cachedMultiplier;

        xRotation = Mathf.Clamp(xRotation - delta.y, minY, maxY);
        yRotation += delta.x; 
        yawPivot.localRotation   = Quaternion.AngleAxis(yRotation, AxisY);
        pitchPivot.localRotation = Quaternion.AngleAxis(xRotation, AxisX);
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}