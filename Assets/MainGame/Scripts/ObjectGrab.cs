using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class ObjectGrab : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
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
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private bool rotateAroundWorldUp = true;
    [SerializeField] private float scrollStep = 0.25f;
    [SerializeField] private float maxScrollDistance = 2f;
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
    private Transform heldTransform;
    private bool isPullingIn;
    private Vector3 trackedVelocity;
    private float scrollOffset;
    private bool isCharging;
    private float chargeNormalized;
    private bool hasHoldPoint;
    private bool hasCamera;
    private bool hasChargeSlider;
    private bool hasAudio;
    private bool hasShake;
    private Transform cameraTransform;

    private void Awake()
    {
        hasHoldPoint = holdPoint != null;
        hasCamera    = playerCamera != null;
        hasChargeSlider = chargeSlider != null;
        hasAudio     = audioSource != null;
        hasShake     = cameraShake != null;
        cameraTransform = hasCamera ? playerCamera.transform : null;

        if (uiElementsToFade != null)
        {
            int len = uiElementsToFade.Length;
            for (int i = 0; i < len; i++)
            {
                var g = uiElementsToFade[i];
                if (g == null) continue;
                Color c = g.color;
                c.a = 0f;
                g.color = c;
            }
        }
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (heldObject == null)
        {
            if (kb.eKey.wasPressedThisFrame) TryGrab();
            return;
        }

        if (kb.eKey.wasReleasedThisFrame)
        {
            ReleaseObject();
            return;
        }

        if (!hasHoldPoint) return;
        HandleRotation(kb);
        var mouse = Mouse.current;
        if (mouse != null && hasCamera)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                scrollOffset += Mathf.Sign(scroll) * scrollStep;
                if (scrollOffset > maxScrollDistance) scrollOffset = maxScrollDistance;
                else if (scrollOffset < -maxScrollDistance) scrollOffset = -maxScrollDistance;
            }
        }
        HandleCharge(kb);
    }

    private void FixedUpdate()
    {
        if (heldObject != null && hasHoldPoint)
            FollowHoldPoint(Time.fixedDeltaTime);
    }

    private void HandleRotation(Keyboard kb)
    {
        float yawDir = 0f;
        if (kb.aKey.isPressed) yawDir += 1f;
        if (kb.dKey.isPressed) yawDir -= 1f;

        float pitchDir = 0f;
        if (kb.wKey.isPressed) pitchDir -= 1f;
        if (kb.sKey.isPressed) pitchDir += 1f;

        if (yawDir == 0f && pitchDir == 0f) return;
        float delta = rotateSpeed * Time.deltaTime;
        Vector3 yawAxis;
        if (rotateAroundWorldUp) yawAxis = Vector3.up;
        else if (cameraTransform != null) yawAxis = cameraTransform.up;
        else yawAxis = Vector3.up;
        Vector3 pitchAxis = cameraTransform != null ? cameraTransform.right : Vector3.right;
        Quaternion yawRot   = yawDir   != 0f ? Quaternion.AngleAxis(yawDir   * delta, yawAxis)   : Quaternion.identity;
        Quaternion pitchRot = pitchDir != 0f ? Quaternion.AngleAxis(pitchDir * delta, pitchAxis) : Quaternion.identity;
        Quaternion targetRot = yawRot * pitchRot * heldRb.rotation;
        heldRb.MoveRotation(targetRot);
    }

    // -------- Захват --------
    private void TryGrab()
    {
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabLayerMask)) return;
        if (!hit.collider.CompareTag("Pickable")) return;
        if (!hit.collider.TryGetComponent(out Rigidbody rb)) return;

        heldObject    = hit.collider.gameObject;
        heldRb        = rb;
        heldTransform = heldObject.transform;

        heldRb.isKinematic       = false;
        heldRb.detectCollisions  = true;
        heldRb.interpolation     = RigidbodyInterpolation.Interpolate;
        heldRb.useGravity        = false;
        heldRb.linearDamping     = 0f;
        heldRb.angularDamping    = 0f;
        heldRb.constraints       = RigidbodyConstraints.FreezeRotation;
        heldRb.angularVelocity   = Vector3.zero;

        trackedVelocity = Vector3.zero;
        isPullingIn     = true;
        scrollOffset    = 0f;
    }
    private void FollowHoldPoint(float dt)
    {
        Vector3 currentPos = heldRb.position;
        Vector3 targetPos  = holdPoint.position;

        if (cameraTransform != null && scrollOffset != 0f)
            targetPos += cameraTransform.forward * scrollOffset;

        float speed = isPullingIn ? pullInSpeed : followSmoothSpeed;

        // sqrMagnitude вместо Distance
        float dx = currentPos.x - targetPos.x;
        float dy = currentPos.y - targetPos.y;
        float dz = currentPos.z - targetPos.z;
        float sqrDist = dx * dx + dy * dy + dz * dz;
        float sqrLeash = leashDistance * leashDistance;

        float t = 1f - Mathf.Exp(-speed * dt);
        if (sqrDist > sqrLeash)
        {
            float extraT = 1f - Mathf.Exp(-leashExtraSpeed * dt);
            if (extraT > t) t = extraT;
        }

        Vector3 desiredPos      = Vector3.LerpUnclamped(currentPos, targetPos, t);
        Vector3 desiredVelocity = (desiredPos - currentPos) / dt;
        Vector3 velocityDelta   = desiredVelocity - heldRb.linearVelocity;

        heldRb.AddForce(velocityDelta, ForceMode.VelocityChange);

        trackedVelocity = heldRb.linearVelocity;

        if (isPullingIn && sqrDist < 0.0225f) // 0.15^2
            isPullingIn = false;

        if (followSmoothRotation > 0f)
        {
            float rt = 1f - Mathf.Exp(-followSmoothRotation * dt);
            heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, holdPoint.rotation, rt));
        }
    }
    private void HandleCharge(Keyboard kb)
    {
        bool fHeld = kb.fKey.isPressed;

        if (fHeld)
        {
            if (!isCharging)
            {
                isCharging = true;
                chargeNormalized = 0f;

                FadeUI(1f);

                if (hasChargeSlider)
                {
                    chargeSlider.DOKill(false);
                    chargeSlider.value = 0f;
                    chargeSlider.DOValue(1f, chargeDuration).SetEase(Ease.Linear);
                }

                if (hasShake)
                    cameraShake.StartCharge(maxShakeAmplitude, chargeDuration);
            }

            if (hasChargeSlider)
                chargeNormalized = chargeSlider.value;
        }
        else if (isCharging)
        {
            isCharging = false;
            chargeNormalized = 0f;

            if (hasChargeSlider)
            {
                chargeSlider.DOKill(false);
                chargeSlider.DOValue(0f, uiFadeDuration);
            }

            FadeUI(0f);

            if (hasShake) cameraShake.StopCharge();
        }
    }

    private void FadeUI(float targetAlpha)
    {
        if (uiElementsToFade == null) return;
        int len = uiElementsToFade.Length;
        for (int i = 0; i < len; i++)
        {
            var g = uiElementsToFade[i];
            if (g == null) continue;
            g.DOKill(false);
            g.DOFade(targetAlpha, uiFadeDuration);
        }
    }
    private void ReleaseObject()
    {
        if (heldObject == null) return;

        bool  wasCharging    = isCharging;
        float chargeAtRelease = chargeNormalized;

        if (isCharging)
        {
            isCharging = false;
            chargeNormalized = 0f;

            if (hasChargeSlider)
            {
                chargeSlider.DOKill(false);
                chargeSlider.value = 0f;
            }

            FadeUI(0f);

            if (hasShake) cameraShake.StopCharge();
        }

        if (heldRb != null)
        {
            heldRb.isKinematic      = false;
            heldRb.detectCollisions = true;
            heldRb.interpolation    = RigidbodyInterpolation.Interpolate;
            heldRb.useGravity       = true;
            heldRb.linearDamping    = 0f;
            heldRb.angularDamping   = 0.05f;
            heldRb.constraints      = RigidbodyConstraints.None;

            if (wasCharging)
            {
                if (hasAudio) audioSource.Play();
                float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeAtRelease);
                Vector3 dir = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
                heldRb.linearVelocity = dir * throwForce;

                if (hasShake)
                    cameraShake.Shake(releaseShakeAmplitude, releaseShakeDuration);
            }
            else
            {
                heldRb.linearVelocity = trackedVelocity * throwSpeedMultiplier;
            }
        }

        heldObject    = null;
        heldRb        = null;
        heldTransform = null;
        isPullingIn   = false;
        trackedVelocity = Vector3.zero;
        scrollOffset  = 0f;
    }
}