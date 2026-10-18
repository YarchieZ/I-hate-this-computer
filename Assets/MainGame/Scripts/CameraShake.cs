using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    [Header("Настройки тряски")]
    [Tooltip("Частота колебаний во время зажатия F")]
    public float shakeFrequency = 12f;
    
    [Tooltip("Частота колебаний при ударе")]
    public float traumaFrequency = 25f;
    
    [Tooltip("Множитель вращения (углы)")]
    public float rotationMultiplier = 25f;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    
    private Tween chargeTween;
    private Tween releaseTween;
    
    private float chargeAmplitude;
    private float releaseAmplitude;
    
    private float chargeProgress;
    private float maxChargeAmplitude;

    private void Awake()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
    }

    private void Update()
    {
        // 1. Амплитуда заряда (плавно нарастает)
        if (chargeProgress > 0f)
        {
            float breath = Mathf.Sin(Time.time * 4f) * 0.3f + 0.7f;
            chargeAmplitude = maxChargeAmplitude * chargeProgress * breath;
        }
        else
        {
            chargeAmplitude = 0f;
        }

        float totalAmplitude = chargeAmplitude + releaseAmplitude;

        if (totalAmplitude <= 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPos, Time.deltaTime * 15f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalLocalRot, Time.deltaTime * 15f);
            return;
        }

        // 2. Используем синусоиды с разными частотами для "вязкой" тряски
        float t1 = Time.time * shakeFrequency;
        float t2 = Time.time * traumaFrequency;
        float t = Mathf.Lerp(t1, t2, releaseAmplitude / (totalAmplitude + 0.001f));

        // Смещение (позиция)
        float x = (Mathf.Sin(t * 1.1f) + Mathf.Sin(t * 1.7f)) * 0.5f;
        float y = (Mathf.Cos(t * 1.3f) + Mathf.Cos(t * 1.9f)) * 0.5f;
        
        // Вращение (наклоны)
        float rotX = Mathf.Sin(t * 1.5f) * totalAmplitude * rotationMultiplier;
        float rotY = Mathf.Cos(t * 1.4f) * totalAmplitude * rotationMultiplier;
        float rotZ = Mathf.Sin(t * 0.9f) * totalAmplitude * rotationMultiplier * 1.5f;

        // Применяем. Множитель 5f нужен, чтобы позиция была заметна (в метрах)
        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f) * totalAmplitude * 5f;
        transform.localRotation = originalLocalRot * Quaternion.Euler(rotX, rotY, rotZ);
    }

    public void StartCharge(float maxAmplitude, float duration)
    {
        chargeTween?.Kill();
        chargeProgress = 0f;
        maxChargeAmplitude = maxAmplitude;

        chargeTween = DOTween.To(
            x => chargeProgress = x, 
            0f,
            1f,
            duration
        ).SetEase(Ease.InQuad);
    }

    public void StopCharge()
    {
        chargeTween?.Kill();
        chargeTween = null;
        chargeProgress = 0f;
        chargeAmplitude = 0f;
    }

    public void Shake(float maxAmplitude, float duration)
    {
        releaseTween?.Kill();
        
        releaseAmplitude = maxAmplitude;

        releaseTween = DOTween.To(
            x => releaseAmplitude = x,
            maxAmplitude,
            0f,
            duration
        ).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            releaseTween = null;
        });
    }
}