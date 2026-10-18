using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SpawnOptions
{
    Keyboard,
    Monitor,
    Note
}

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private GameObject keyboardPrefab;
    [SerializeField] private GameObject monitorPrefab;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform keyboardStartPoint;
    [SerializeField] private Transform monitorStartPoint;
    [SerializeField] private List<string> notes;
    public SpawnOptions spawnOptions;
    private int completedTasks = -1;
    public MonitorStatus currentMonitor {get; private set;}
    public NotePadSpawner currentNotePad {get; private set; }
    public KeyCapManager currentKeyboard { get; private set; }
    public string currentNoteName {get; private set;}

    private void Start()
    {
        StartCoroutine(StartSpawn());
    }

    public void SpawnItem(SpawnOptions option, Transform spawnPos = null)
    {
        if (spawnPos == null)
        {
            spawnPos = spawnPosition;
        }
        if (option == SpawnOptions.Keyboard)
        {
            GameObject obj = Instantiate(keyboardPrefab, spawnPos.position, spawnPos.rotation);
            currentKeyboard = obj.GetComponent<KeyCapManager>();
            
            // Если монитор уже есть, сразу связываем
            if (currentKeyboard != null && currentMonitor != null)
            {
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera);
            }
        }
        else if (option == SpawnOptions.Monitor)
        {
            GameObject obj = Instantiate(monitorPrefab, spawnPos.position, spawnPos.rotation);
            currentMonitor = obj.GetComponent<MonitorStatus>();
            
            if (currentMonitor != null)
            {
                currentMonitor.OnSpawn(this);
                
                // Если клавиатура уже заспавнилась раньше, сообщаем ей о новом мониторе
                if (currentKeyboard != null)
                {
                    currentKeyboard.UpdateInfo(currentMonitor, mainCamera);
                }
            }
        }
        else if (option == SpawnOptions.Note)
        {
            completedTasks++;
            textMesh.text = $"{completedTasks.ToString()}/10";
            GameObject currentNote = Instantiate(notePrefab, spawnPos.position, spawnPos.rotation);
            currentNotePad = currentNote.GetComponent<NotePadSpawner>();
            if (currentNotePad != null)
            {
                int randomNumber = Random.Range(0, notes.Count);
                currentNoteName = notes[randomNumber];
                currentNotePad.OnSpawn(currentNoteName);
                notes.RemoveAt(randomNumber);
            }
        }
    }
    public void UpdateInfoAbout(SpawnOptions spawnOption)
    {
        if (spawnOption == SpawnOptions.Keyboard)
        {
            if (currentKeyboard != null && currentMonitor != null)
            {
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera);
            }
        }
        else if (spawnOption == SpawnOptions.Monitor)
        {
            if (currentKeyboard != null && currentMonitor != null)
            {
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera);
            }
        }
    }

    private IEnumerator StartSpawn()
    {
        // Сначала монитор, потом клавиатура
        SpawnItem(SpawnOptions.Monitor, monitorStartPoint);
        yield return new WaitForSeconds(1f);
        SpawnItem(SpawnOptions.Keyboard, keyboardStartPoint);
        yield return new WaitForSeconds(1f);
        loadingScreen.HideLoadingPanel();
        SpawnItem(SpawnOptions.Note, spawnPosition);
    }
}