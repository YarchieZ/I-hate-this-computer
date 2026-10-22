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
    
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private TextMeshPro textMesh;
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
    public bool isMonitorBroken {get; private set;}
    public bool isKeyboardBroken {get; private set;}
    public MonitorStatus currentMonitor {get; private set;}
    public NotePadSpawner currentNotePad {get; private set; }
    public KeyCapManager currentKeyboard { get; private set; }
    public string currentNoteName {get; private set;}

    private void Start()
    {
        StartCoroutine(StartSpawn());
    }

    public void OnObjectBroken(SpawnOptions spawnOption)
    {
        if (spawnOption == SpawnOptions.Keyboard)
        {
            isKeyboardBroken = true;
        }
        else if (spawnOption == SpawnOptions.Monitor)
        {
            isMonitorBroken = true;
        }
    }

    public void SpawnItem(SpawnOptions option, Transform spawnPos = null)
    {
        if (spawnPos == null)
        {
            spawnPos = spawnPosition;
        }
        if (option == SpawnOptions.Keyboard)
        {
            isKeyboardBroken = false;
            GameObject obj = Instantiate(keyboardPrefab, spawnPos.position, keyboardPrefab.transform.rotation);
            currentKeyboard = obj.GetComponent<KeyCapManager>();
            if (currentKeyboard != null && currentMonitor != null)
            {
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera, this);
            }
        }
        else if (option == SpawnOptions.Monitor)
        {
            isMonitorBroken = false;
            GameObject obj = Instantiate(monitorPrefab, spawnPos.position, monitorPrefab.transform.rotation);
            currentMonitor = obj.GetComponent<MonitorStatus>();
            
            if (currentMonitor != null)
            {
                currentMonitor.OnSpawn(this);
                if (currentKeyboard != null)
                {
                    currentKeyboard.UpdateInfo(currentMonitor, mainCamera, this);
                }
            }
        }
        else if (option == SpawnOptions.Note)
        {
            if (currentNotePad != null)
            {
                currentNotePad.ChangeCurrentNote();
            }
            completedTasks++;
            if (completedTasks >= 2)
            {
                pauseMenu.ShowPause();
            }
            textMesh.text = $"{completedTasks.ToString()}/2";
            GameObject currentNote = Instantiate(notePrefab, spawnPos.position, notePrefab.transform.rotation);
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
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera, this);
            }
        }
        else if (spawnOption == SpawnOptions.Monitor)
        {
            if (currentKeyboard != null && currentMonitor != null)
            {
                currentKeyboard.UpdateInfo(currentMonitor, mainCamera, this);
            }
        }
    }

    private IEnumerator StartSpawn()
    {
        SpawnItem(SpawnOptions.Monitor, monitorStartPoint);
        yield return new WaitForSeconds(1f);
        SpawnItem(SpawnOptions.Keyboard, keyboardStartPoint);
        yield return new WaitForSeconds(1f);
        loadingScreen.HideLoadingPanel();
        SpawnItem(SpawnOptions.Note, spawnPosition);
    }
}