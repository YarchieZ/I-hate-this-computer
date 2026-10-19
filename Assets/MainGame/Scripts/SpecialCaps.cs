using UnityEngine;

public class SpecialCaps : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    private MonitorStatus monitor;
    private AudioSource audioSource;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
    }
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

    public void OnMogged()
    {
        audioSource.PlayOneShot(audioClip);
    }

    public void OnEnter()
    {
        monitor.CheckText();
    }
}