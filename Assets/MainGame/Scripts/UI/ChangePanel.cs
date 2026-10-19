using UnityEngine;
using UnityEngine.UI;

public class ChangePanel : MonoBehaviour
{
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private GameObject newPanel;
    private Button _button;


    private void Awake()
    {
        TryGetComponent<Button>(out _button);
        if (_button != null)
        {
            _button.onClick.AddListener(OnPanelChange);
        }
    }
    private void OnPanelChange()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        if (newPanel != null)
        {
            newPanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (newPanel != null)
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}