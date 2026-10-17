using System;
using UnityEngine;

public enum SpawnOptions
{
    Keyboard,
    Monitor,
    Note
}
public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Transform spawnPosition;
    private NotePadSpawner _currentNotePad;
    private string _currentNoteName;

    private void Start()
    {
        GameObject currentNote = Instantiate(notePrefab, spawnPosition.position, spawnPosition.rotation);
        _currentNotePad = currentNote.GetComponent<NotePadSpawner>();
        if (_currentNotePad != null)
        {
            _currentNotePad.OnSpawn();
            _currentNoteName = _currentNotePad.CurrentSentence;
        }
    }
    public bool CheckNote(string sentence)
    {
        if (sentence == _currentNoteName) return true;
        return false;
    }
    

}
