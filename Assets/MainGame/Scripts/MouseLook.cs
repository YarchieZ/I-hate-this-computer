using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 0.15f; // подгони под себя
    [SerializeField] private float minY = -80f;
    [SerializeField] private float maxY = 80f;
    [SerializeField] private bool lockCursor = true;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Transform yawPivot;
    private Transform pitchPivot;

    void Awake()
    {
        pitchPivot = transform;
        yawPivot = transform.parent;

        // Берём только X пивота камеры; Yaw — только если родитель реально Yaw-пивот.
        xRotation = NormalizeAngle(pitchPivot.localEulerAngles.x);
        yRotation = yawPivot != null ? NormalizeAngle(yawPivot.localEulerAngles.y) : 0f;

        pitchPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (yawPivot != null)
            yawPivot.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (Mouse.current == null || yawPivot == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        // НЕ умножаем на Time.deltaTime — delta уже за кадр
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        yRotation += mouseX;
        if (yRotation > 360f) yRotation -= 360f;
        if (yRotation < -360f) yRotation += 360f;

        yawPivot.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        pitchPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}