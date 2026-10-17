using System;
using UnityEngine;
using DG.Tweening;

public class Tweener : MonoBehaviour
{
    private Transform t;
    public void Test(Action callback = null)
    {
        t.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.Linear).OnComplete(()=> callback?.Invoke());
    }
}
