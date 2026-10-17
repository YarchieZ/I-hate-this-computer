using UnityEngine;
using UnityEngine.InputSystem;

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
    private GameObject heldObject;
    private Rigidbody heldRb;
    private bool isPullingIn;
    private Vector3 lastPosition;
    private Vector3 trackedVelocity;
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
            FollowHoldPoint(Time.deltaTime);
        }
            
    }

    private void TryGrab()
    {
        if (playerCamera == null) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabLayerMask))
        {
            if (!hit.collider.CompareTag("Pickable")) return;
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb == null) return;
            heldObject = hit.collider.gameObject;
            heldRb = rb;
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
            heldRb.interpolation = RigidbodyInterpolation.None;
            lastPosition = heldObject.transform.position;
            trackedVelocity = Vector3.zero;
            isPullingIn = true;
        }
    }

    private void FollowHoldPoint(float dt)
    {
        Vector3 currentPos = heldObject.transform.position;
        Vector3 targetPos = holdPoint.position;
        float speed = isPullingIn ? pullInSpeed : followSmoothSpeed;
        float t = 1f - Mathf.Exp(-speed * dt);
        float distance = Vector3.Distance(currentPos, targetPos);
        if (distance > leashDistance)
        {
            float extraT = 1f - Mathf.Exp(-leashExtraSpeed * dt);
            t = Mathf.Max(t, extraT);
        }
        Vector3 newPos = Vector3.Lerp(currentPos, targetPos, t);
        heldObject.transform.position = newPos;
        if (dt > 0f)
        {
            trackedVelocity = (newPos - lastPosition) / dt;
        }
        lastPosition = newPos;
        if (isPullingIn && distance < 0.15f)
        {
            isPullingIn = false;
        }
        if (followSmoothRotation > 0f)
        {
            float rt = 1f - Mathf.Exp(-followSmoothRotation * dt);
            heldObject.transform.rotation = Quaternion.Slerp(
                heldObject.transform.rotation, holdPoint.rotation, rt);
        }
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;
        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
            heldRb.interpolation = RigidbodyInterpolation.Interpolate;
            heldRb.linearVelocity = trackedVelocity * throwSpeedMultiplier;
        }

        heldObject = null;
        heldRb = null;
        isPullingIn = false;
        trackedVelocity = Vector3.zero;
    }
}