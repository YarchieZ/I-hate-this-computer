using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BreakableObject : MonoBehaviour
{
    [Header("Настройки разрушения")]
    [SerializeField] private float breakSpeedThreshold = 5f;
    [SerializeField] private bool useRelativeVelocity = true;
    [SerializeField] private float checkCooldown = 0.05f;

    [SerializeField] private AudioClip audioClip;
    
    [Header("Тайминги")]
    [SerializeField] private float immunityDuration = 5f;
    [SerializeField] private float brokenFlagDelay = 1f;
    [SerializeField] private float disableComponentsDelay = 0f;

    [Header("Компоненты для отключения при поломке")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;

    [Header("Материал при поломке")]
    [SerializeField] private Material brokenMaterial;
    [SerializeField] private List<Renderer> renderersToChangeMaterial = new List<Renderer>();

    [Header("Состояние")]
    [SerializeField] private bool isBroken = false;

    public bool IsBroken => isBroken;
    public bool AlreadyGettingDestroyed;
    public event Action OnBroken;
    private AudioSource _audioSource;
    private Rigidbody _rb;
    private float _lastCheckTime;
    private float _spawnTime;
    private bool _breakTriggered;
    private Coroutine _brokenFlagCoroutine;
    private Coroutine _disableComponentsCoroutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        _spawnTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_breakTriggered) return;
        if (Time.time - _spawnTime < immunityDuration) return;
        if (Time.time - _lastCheckTime < checkCooldown) return;
        _lastCheckTime = Time.time;

        float impactSpeed = CalculateImpactSpeed(collision);

        if (impactSpeed >= breakSpeedThreshold)
        {
            Break();
        }
    }

    private float CalculateImpactSpeed(Collision collision)
    {
        Vector3 myVelocity = _rb.linearVelocity;

        if (!useRelativeVelocity)
            return myVelocity.magnitude;

        Rigidbody otherRb = collision.rigidbody;
        Vector3 otherVelocity = otherRb != null ? otherRb.linearVelocity : Vector3.zero;

        Vector3 normal = collision.contactCount > 0
            ? collision.GetContact(0).normal
            : Vector3.zero;

        Vector3 relativeVelocity = myVelocity - otherVelocity;
        float normalSpeed = Mathf.Abs(Vector3.Dot(relativeVelocity, normal));

        return normal == Vector3.zero ? relativeVelocity.magnitude : normalSpeed;
    }

    public void Break()
    {
        if (_breakTriggered) return;
        _breakTriggered = true;
        Debug.Log("Break");
        OnBroken?.Invoke();
        _audioSource.PlayOneShot(audioClip);
        ApplyBrokenMaterial();

        if (disableComponentsDelay > 0f)
        {
            if (_disableComponentsCoroutine != null)
                StopCoroutine(_disableComponentsCoroutine);
            _disableComponentsCoroutine = StartCoroutine(DisableComponentsAfterDelay(disableComponentsDelay));
        }
        else
        {
            DisableComponents();
        }

        if (_brokenFlagCoroutine != null)
            StopCoroutine(_brokenFlagCoroutine);
        _brokenFlagCoroutine = StartCoroutine(SetBrokenAfterDelay(brokenFlagDelay));
    }

    private void ApplyBrokenMaterial()
    {
        if (brokenMaterial == null) return;
        if (renderersToChangeMaterial == null) return;

        foreach (var rend in renderersToChangeMaterial)
        {
            if (rend == null) continue;

            Material[] materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = brokenMaterial;
            }
            rend.materials = materials;
        }
    }

    private void DisableComponents()
    {
        if (componentsToDisable == null) return;
        foreach (var comp in componentsToDisable)
        {
            if (comp != null)
                comp.enabled = false;
        }
    }

    private System.Collections.IEnumerator DisableComponentsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisableComponents();
        _disableComponentsCoroutine = null;
    }

    private System.Collections.IEnumerator SetBrokenAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        isBroken = true;
        _brokenFlagCoroutine = null;
    }

    public void ResetBroken()
    {
        if (_brokenFlagCoroutine != null)
        {
            StopCoroutine(_brokenFlagCoroutine);
            _brokenFlagCoroutine = null;
        }
        if (_disableComponentsCoroutine != null)
        {
            StopCoroutine(_disableComponentsCoroutine);
            _disableComponentsCoroutine = null;
        }
        isBroken = false;
        _breakTriggered = false;
        _spawnTime = Time.time;
    }
}