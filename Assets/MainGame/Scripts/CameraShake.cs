using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    [Header("Настройки тряски")]
    public float shakeFrequency = 12f;
    public float traumaFrequency = 25f;
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

        // ВАЖНО: Если амплитуда 0, мы НЕ ТРОГАЕМ position и rotation вообще.
        // Это позволяет скрипту мыши свободно вращать камеру.
        if (totalAmplitude <= 0.001f)
        {
            return; 
        }

        float t1 = Time.time * shakeFrequency;
        float t2 = Time.time * traumaFrequency;
        float t = Mathf.Lerp(t1, t2, releaseAmplitude / (totalAmplitude + 0.001f));

        // Смещение (позиция)
        float x = (Mathf.Sin(t * 1.1f) + Mathf.Sin(t * 1.7f)) * 0.5f;
        float y = (Mathf.Cos(t * 1.3f) + Mathf.Cos(t * 1.9f)) * 0.5f;
        
        // Вращение (наклоны) - применяем только если есть амплитуда
        float rotX = Mathf.Sin(t * 1.5f) * totalAmplitude * rotationMultiplier;
        float rotY = Mathf.Cos(t * 1.4f) * totalAmplitude * rotationMultiplier;
        float rotZ = Mathf.Sin(t * 0.9f) * totalAmplitude * rotationMultiplier * 1.5f;

        // Смещаем позицию
        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f) * totalAmplitude * 5f;
        
        // Накладываем вращение поверх текущего (чтобы не сбивать мышь)
        transform.localRotation = transform.localRotation * Quaternion.Euler(rotX, rotY, rotZ);
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
        transform.localPosition = originalLocalPos; // Возвращаем в исходную точку при отпускании
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