using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio : MonoBehaviour
{
    [Header("Music Settings")]
    public List<AudioClip> musicTracks = new List<AudioClip>();
    [Range(0f, 1f)] public float volume = 0.5f;
    public float fadeDuration = 1f;
    public bool shuffle = true;
    public bool loop = true;

    private AudioSource audioSource;
    private List<AudioClip> playlist = new List<AudioClip>();
    private int currentTrackIndex = 0;
    private bool isFading = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0; // Start at 0 for fade in
        audioSource.loop = false; // We'll handle looping manually

        if (musicTracks.Count > 0)
        {
            InitializePlaylist();
            PlayNextTrack();
        }
        else
        {
            Debug.LogWarning("No music tracks assigned to MusicPlayer");
        }
    }

    void InitializePlaylist()
    {
        playlist = new List<AudioClip>(musicTracks);
        if (shuffle)
        {
            playlist = playlist.OrderBy(x => Random.value).ToList();
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && !isFading && playlist.Count > 0)
        {
            PlayNextTrack();
        }
    }

    void PlayNextTrack()
    {
        if (playlist.Count == 0) return;

        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            if (loop)
            {
                currentTrackIndex = 0;
                if (shuffle) // Reshuffle if looping and shuffle is enabled
                {
                    playlist = playlist.OrderBy(x => Random.value).ToList();
                }
            }
            else
            {
                return; // Don't play if not looping
            }
        }

        audioSource.clip = playlist[currentTrackIndex];
        StartCoroutine(FadeInAndPlay());
    }

    System.Collections.IEnumerator FadeInAndPlay()
    {
        isFading = true;
        audioSource.Play();

        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, volume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
        isFading = false;
    }

    System.Collections.IEnumerator FadeOut()
    {
        isFading = true;
        float startVolume = audioSource.volume;

        // Fade out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0;
        isFading = false;
    }

    public void SkipTrack()
    {
        if (isFading) return;
        StartCoroutine(FadeOutAndPlayNext());
    }

    System.Collections.IEnumerator FadeOutAndPlayNext()
    {
        yield return StartCoroutine(FadeOut());
        PlayNextTrack();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (!isFading)
        {
            audioSource.volume = volume;
        }
    }
}