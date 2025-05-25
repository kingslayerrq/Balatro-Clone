using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("BGM")] 
    public AudioClip mainBGM;
    
    [Header("Button SFX")]
    public AudioClip buttonClick;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if (mainBGM == null || bgmAudioSource == null)
        {
            Debug.LogWarning("Audio Clip not set");
        }
    }

    #region BGM

    public void PlayMainBGM()
    {
        if (bgmAudioSource == null)
        {
            Debug.LogWarning("BGM AudioSource is null");
            return;
        }
        if (bgmAudioSource.isPlaying && bgmAudioSource.clip != null) return;
        bgmAudioSource.clip = mainBGM;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = 0.1f;
        bgmAudioSource.Play();
    }
    

    #endregion

    #region Button SFX

    public void PlayButtonClick()
    {
        if (sfxAudioSource == null)
        {
            Debug.LogWarning("SFX AudioSource is null");
            return;
        }
        sfxAudioSource.PlayOneShot(buttonClick);

    }

    #endregion
}
