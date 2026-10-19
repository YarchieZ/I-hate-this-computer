using System.Collections;
using UnityEngine;

public class BreakTrigger : MonoBehaviour
{
    [SerializeField] private float timeUntilDestroy;
    [SerializeField] private Transform target;

    private void OnTriggerEnter(Collider obj)
    {
        obj.TryGetComponent<BreakableObject>(out BreakableObject breakableObject);
        obj.TryGetComponent<NotePadSpawner>(out NotePadSpawner currentNotePad);
        if (breakableObject != null)
        {
            if (breakableObject.AlreadyGettingDestroyed)
            {
                
            }
            else
            {
                breakableObject.AlreadyGettingDestroyed = true;
                breakableObject.Break();
                Destroy(obj.gameObject, timeUntilDestroy);
            }
        }
        else if (currentNotePad != null)
        {
            if (currentNotePad.isMeCurrent)
            {
                currentNotePad.transform.position = target.position;
            }
            else
            {
                Destroy(obj.gameObject, timeUntilDestroy);
            }
        }
    }
    
    
}
