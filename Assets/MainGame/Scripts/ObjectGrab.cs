using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class ObjectGrab : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private LayerMask grabLayerMask = ~0;
    [SerializeField] private float followSmoothSpeed = 4f;
    [SerializeField] private float followSmoothRotation = 0f;
    [SerializeField] private float pullInSpeed = 2.5f;
    [SerializeField] private float leashDistance = 4f;
    [SerializeField] private float leashExtraSpeed = 8f;
    [SerializeField] private float throwSpeedMultiplier = 1f;

    [Header("Вращение в руках")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private bool rotateAroundWorldUp = true;

    [Header("Дистанция колесиком")]
    [SerializeField] private float scrollStep = 0.25f;
    [SerializeField] private float maxScrollDistance = 2f;

    [Header("Заряд броска (F)")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Graphic[] uiElementsToFade;
    [SerializeField] private float chargeDuration = 3f;
    [SerializeField] private float uiFadeDuration = 0.3f;
    [SerializeField] private float minThrowForce = 6f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float maxShakeAmplitude = 0.25f;
    [SerializeField] private float releaseShakeAmplitude = 0.5f;
    [SerializeField] private float releaseShakeDuration = 0.5f;
    [SerializeField] private CameraShake cameraShake;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private bool isPullingIn;
    private Vector3 lastPosition;
    private Vector3 trackedVelocity;
    private float scrollOffset;

    private bool isCharging;
    private float chargeNormalized;

    private void Start()
    {
        if (uiElementsToFade != null)
        {
            for (int i = 0; i < uiElementsToFade.Length; i++)
            {
                if (uiElementsToFade[i] != null)
                {
                    Color c = uiElementsToFade[i].color;
                    c.a = 0f;
                    uiElementsToFade[i].color = c;
                }
            }
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && heldObject == null)
        {
            TryGrab();
        }

        if (Keyboard.current.eKey.wasReleasedThisFrame && heldObject != null)
        {
            ReleaseObject();
        }

        if (heldObject != null && holdPoint != null)
        {
            HandleRotation();
            HandleScroll();
            HandleCharge();
        }
    }

    private void FixedUpdate()
    {
        if (heldObject != null && holdPoint != null)
        {
            FollowHoldPoint(Time.fixedDeltaTime);
        }
    }

    private void HandleScroll()
    {
        if (Mouse.current == null || playerCamera == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll == 0f) return;

        scrollOffset += Mathf.Sign(scroll) * scrollStep;
        scrollOffset = Mathf.Clamp(scrollOffset, -maxScrollDistance, maxScrollDistance);
    }

    private void HandleCharge()
    {
        bool fHeld = Keyboard.current.fKey.isPressed;

        if (fHeld)
        {
            if (!isCharging)
            {
                isCharging = true;
                chargeNormalized = 0f;

                FadeUI(1f);

                if (chargeSlider != null)
                {
                    chargeSlider.DOKill();
                    chargeSlider.value = 0f;
                    chargeSlider.DOValue(1f, chargeDuration).SetEase(Ease.Linear);
                }

                if (cameraShake != null)
                {
                    cameraShake.StartCharge(maxShakeAmplitude, chargeDuration);
                }
            }

            if (chargeSlider != null)
            {
                chargeNormalized = chargeSlider.value;
            }
        }
        else if (isCharging)
        {
            isCharging = false;
            chargeNormalized = 0f;

            if (chargeSlider != null)
            {
                chargeSlider.DOKill();
                chargeSlider.DOValue(0f, uiFadeDuration);
            }

            FadeUI(0f);

            if (cameraShake != null)
            {
                cameraShake.StopCharge();
            }
        }
    }

    private void FadeUI(float targetAlpha)
    {
        if (uiElementsToFade == null) return;

        for (int i = 0; i < uiElementsToFade.Length; i++)
        {
            if (uiElementsToFade[i] != null)
            {
                uiElementsToFade[i].DOKill();
                uiElementsToFade[i].DOFade(targetAlpha, uiFadeDuration);
            }
        }
    }

    private void HandleRotation()
    {
        float yawDirection = 0f;
        if (Keyboard.current.aKey.isPressed) yawDirection += 1f;
        if (Keyboard.current.dKey.isPressed) yawDirection -= 1f;

        float pitchDirection = 0f;
        if (Keyboard.current.wKey.isPressed) pitchDirection -= 1f;
        if (Keyboard.current.sKey.isPressed) pitchDirection += 1f;

        if (yawDirection == 0f && pitchDirection == 0f) return;

        Vector3 yawAxis = rotateAroundWorldUp
            ? Vector3.up
            : playerCamera != null ? playerCamera.transform.up : Vector3.up;

        float delta = rotateSpeed * Time.deltaTime;

        if (yawDirection != 0f)
        {
            heldObject.transform.Rotate(yawAxis, yawDirection * delta, Space.World);
        }

        if (pitchDirection != 0f)
        {
            Vector3 pitchAxis = hasCameraAxis()
                ? playerCamera.transform.right
                : Vector3.right;
            heldObject.transform.Rotate(pitchAxis, pitchDirection * delta, Space.World);
        }
    }

    private bool hasCameraAxis()
    {
        return playerCamera != null;
    }

    private void TryGrab()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabLayerMask))
        {
            if (!hit.collider.CompareTag("Pickable")) return;
            hit.collider.TryGetComponent(out Rigidbody rb);
            heldObject = hit.collider.gameObject;
            heldRb = rb;
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
            heldRb.interpolation = RigidbodyInterpolation.Interpolate;
            heldRb.useGravity = false;
            heldRb.linearDamping = 0f;
            heldRb.angularDamping = 0f;
            heldRb.constraints = RigidbodyConstraints.FreezeRotation;
            heldRb.angularVelocity = Vector3.zero;
            lastPosition = heldObject.transform.position;
            trackedVelocity = Vector3.zero;
            isPullingIn = true;
            scrollOffset = 0f;
        }
    }

    private void FollowHoldPoint(float dt)
    {
        Vector3 currentPos = heldRb.position;
        Vector3 targetPos = holdPoint.position;

        if (playerCamera != null && scrollOffset != 0f)
        {
            targetPos += playerCamera.transform.forward * scrollOffset;
        }

        float speed = isPullingIn ? pullInSpeed : followSmoothSpeed;
        float t = 1f - Mathf.Exp(-speed * dt);

        float distance = Vector3.Distance(currentPos, targetPos);
        if (distance > leashDistance)
        {
            float extraT = 1f - Mathf.Exp(-leashExtraSpeed * dt);
            t = Mathf.Max(t, extraT);
        }

        Vector3 desiredPos = Vector3.Lerp(currentPos, targetPos, t);
        Vector3 desiredVelocity = (desiredPos - currentPos) / dt;
        Vector3 velocityDelta = desiredVelocity - heldRb.linearVelocity;

        heldRb.AddForce(velocityDelta, ForceMode.VelocityChange);

        if (dt > 0f)
        {
            trackedVelocity = heldRb.linearVelocity;
        }
        lastPosition = heldRb.position;

        if (isPullingIn && distance < 0.15f)
        {
            isPullingIn = false;
        }

        if (followSmoothRotation > 0f)
        {
            float rt = 1f - Mathf.Exp(-followSmoothRotation * dt);
            heldRb.MoveRotation(Quaternion.Slerp(
                heldRb.rotation, holdPoint.rotation, rt));
        }
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;

        bool wasCharging = isCharging;
        float chargeAtRelease = chargeNormalized;

        if (isCharging)
        {
            isCharging = false;
            chargeNormalized = 0f;

            if (chargeSlider != null)
            {
                chargeSlider.DOKill();
                chargeSlider.value = 0f;
            }

            FadeUI(0f);

            if (cameraShake != null)
            {
                cameraShake.StopCharge();
            }
        }

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
            heldRb.interpolation = RigidbodyInterpolation.Interpolate;
            heldRb.useGravity = true;
            heldRb.linearDamping = 0f;
            heldRb.angularDamping = 0.05f;
            heldRb.constraints = RigidbodyConstraints.None;

            if (wasCharging)
            {
                float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeAtRelease);
                Vector3 dir = playerCamera != null ? playerCamera.transform.forward : Vector3.forward;
                heldRb.linearVelocity = dir * throwForce;

                if (cameraShake != null)
                {
                    cameraShake.Shake(releaseShakeAmplitude, releaseShakeDuration);
                }
            }
            else
            {
                heldRb.linearVelocity = trackedVelocity * throwSpeedMultiplier;
            }
        }

        heldObject = null;
        heldRb = null;
        isPullingIn = false;
        trackedVelocity = Vector3.zero;
        scrollOffset = 0f;
    }
}