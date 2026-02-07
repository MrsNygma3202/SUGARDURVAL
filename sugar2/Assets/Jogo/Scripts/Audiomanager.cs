using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Músicas")]
    public AudioClip normalMusic;
    public AudioClip specialMusic; // Toca quando olha para o objeto

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
        }
        
        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (normalMusic != null && _audioSource.clip != normalMusic)
        {
            _audioSource.clip = normalMusic;
            _audioSource.Play();
        }
    }

    public void PlaySpecialMusic()
    {
        if (specialMusic != null && _audioSource.clip != specialMusic)
        {
            _audioSource.clip = specialMusic;
            _audioSource.Play();
        }
    }
}