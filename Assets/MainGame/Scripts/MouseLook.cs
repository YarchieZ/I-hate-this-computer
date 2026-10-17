using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minY = -80f;
    [SerializeField] private float maxY = 80f;
    [SerializeField] private bool lockCursor = true;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Transform yawPivot;
    private Transform pitchPivot;

    void Awake()
    {
        yawPivot = transform.parent;
        pitchPivot = transform;
        Vector3 startEuler = pitchPivot.localEulerAngles;
        xRotation = NormalizeAngle(startEuler.x);
        yRotation = NormalizeAngle(yawPivot.localEulerAngles.y);
        pitchPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
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
        if (Mouse.current == null) return;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;
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
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }
}