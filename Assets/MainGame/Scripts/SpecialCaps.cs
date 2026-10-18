using UnityEngine;

public class SpecialCaps : MonoBehaviour
{
    private MonitorStatus monitor;

    public void UpdateInfo(MonitorStatus ss)
    {
        monitor = ss;
    }

    public void OnBackspace()
    {
        string currentText = monitor.GetText();
        if (string.IsNullOrEmpty(currentText)) return;
        int newLength = Mathf.Max(0, currentText.Length - 2); 
        string text = currentText.Substring(0, newLength);
        monitor.UpdateUI(text, false);
    }
}