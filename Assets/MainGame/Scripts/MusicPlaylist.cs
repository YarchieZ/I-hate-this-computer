using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlaylist : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    private AudioSource audioSource;

    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayMusic(clips[Random.Range(0, clips.Length)]));
    }
    private IEnumerator PlayMusic(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        audioSource.clip = clip;
        AudioClip newClip = clips[Random.Range(0, clips.Length)];
        while (clip == newClip)
        {
            newClip = clips[Random.Range(0, clips.Length)];
        }
        StartCoroutine(PlayMusic(newClip));
    }
    
}