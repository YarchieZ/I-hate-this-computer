using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact();
}

public class ClickInteractor : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _maxDistance = 100f;
    [SerializeField] private LayerMask _layerMask = ~0; // по умолчанию всё

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        // Обрабатываем только момент нажатия левой кнопки мыши
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(Mouse.current.position.ReadValue());
        }
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = _camera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _layerMask))
        {
            // Проверяем тег на самом объекте или на родителях (удобно для вложенных моделей)
            if (TryGetInteractable(hit.collider, out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
    }

    private bool TryGetInteractable(Collider collider, out IInteractable interactable)
    {
        interactable = null;

        // Ищем компонент, реализующий IInteractable, вверх по иерархии
        var behaviours = collider.GetComponentsInParent<MonoBehaviour>();
        foreach (var b in behaviours)
        {
            if (b is IInteractable i)
            {
                interactable = i;
                return true;
            }
        }
        return false;
    }
}