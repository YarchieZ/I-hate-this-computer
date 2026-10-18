using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonSpawn : MonoBehaviour, IInteractable
{
    [SerializeField] private bool monitor;
    public void Interact()
    {
        Debug.Log("Дверь открыта!");
        // своя логика: анимация, поворот и т.д.
    }
}