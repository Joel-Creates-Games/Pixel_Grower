using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    [Header("Assign your tracks here")]
    [Tooltip("List of AudioClips to loop through")]
    public AudioClip[] songs = new AudioClip[3];

    private AudioSource _audioSource;
    private int _currentIndex = 0;

    private void Awake()
    {
        // Singleton pattern: keep just one MusicController alive
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Cache or add an AudioSource
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    private void Start()
    {
        if (songs == null || songs.Length == 0)
        {
            Debug.LogWarning("MusicController: No songs assigned.", this);
            return;
        }

        StartCoroutine(PlaySongsLoop());
    }

    private IEnumerator PlaySongsLoop()
    {
        while (true)
        {
            // Assign next clip and play
            _audioSource.clip = songs[_currentIndex];
            _audioSource.Play();

            // Wait exactly until clip ends
            yield return new WaitForSeconds(_audioSource.clip.length);

            // Advance index (wrap-around)
            _currentIndex = (_currentIndex + 1) % songs.Length;
        }
    }
}
