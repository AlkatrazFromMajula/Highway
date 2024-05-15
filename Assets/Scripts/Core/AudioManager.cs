using ScriptableObjects;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Data Containers")]
    [SerializeField] AudioDataContainerSO audioData;

    [Header("Event Channels")]
    [SerializeField] VoidEventChannelSO beginGameEventChannel;
    [SerializeField] VoidEventChannelSO muteAudioEventChannel;
    [SerializeField] VoidEventChannelSO collectCoinEventChannel;
    [SerializeField] VoidEventChannelSO pressButtonEventChannel;
    [SerializeField] BoolEventChannelSO radioEventChannel;
    [SerializeField] FloatEventChannelSO playerSpeedChangeEventChannel;

    [Header("Components")]
    [SerializeField] TextMeshProUGUI radio_TMP;

    [Header("Rest")]
    [SerializeField] AudioMixer masterMixer;

    AudioSource musicSource;
    AudioSource engineSource;
    AudioSource freeSource;

    Coroutine readRadioInput;
    bool readingRadioInput;
    bool mute;

    void OnEnable()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        musicSource = audioSources[0];
        engineSource = audioSources[1];
        freeSource = audioSources[2];

        musicSource.clip = audioData.GetSelectedMusicClip();
        musicSource.Play();
        UpdateRadioLabel();

        musicSource.clip = audioData.GetSelectedMusicClip();

        if (muteAudioEventChannel != null)
            muteAudioEventChannel.OnEventRaised += Un_MuteAudio;
        if (beginGameEventChannel != null)
            beginGameEventChannel.OnEventRaised += BeginGame;
        if (collectCoinEventChannel != null)
            collectCoinEventChannel.OnEventRaised += PlayCollectCoinSound;
        if (pressButtonEventChannel != null)
            pressButtonEventChannel.OnEventRaised += PlayPressButtonSound;
        if (radioEventChannel != null)
            radioEventChannel.OnEventRaised += UpdateRadio;
    }

    void OnDisable()
    {
        if (muteAudioEventChannel != null)
            muteAudioEventChannel.OnEventRaised -= Un_MuteAudio;
        if (beginGameEventChannel != null)
            beginGameEventChannel.OnEventRaised -= BeginGame;
        if (collectCoinEventChannel != null)
            collectCoinEventChannel.OnEventRaised -= PlayCollectCoinSound;
        if (pressButtonEventChannel != null)
            pressButtonEventChannel.OnEventRaised -= PlayPressButtonSound;
        if (radioEventChannel != null)
            radioEventChannel.OnEventRaised -= UpdateRadio;
        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised -= UpdateEngine;
    }

    void Un_MuteAudio()
    {
        mute = !mute;
        float volume = mute ? -20 : 0;
        masterMixer.SetFloat("engine_musicVolume", volume);
    }

    public void BeginGame()
    {
        StartCoroutine(StartEngine());
    }

    IEnumerator StartEngine()
    {
        engineSource.PlayOneShot(audioData.StartEngineAC);
        while (engineSource.isPlaying) 
            yield return new WaitForEndOfFrame();
        engineSource.volume = 0.1f;
        engineSource.clip = audioData.DriveEngineAC;
        engineSource.Play();

        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised += UpdateEngine;
    }

    void PlayCollectCoinSound()
    {
        freeSource.PlayOneShot(audioData.CollectCoinAC);
    }

    void PlayPressButtonSound()
    {
        freeSource.PlayOneShot(audioData.PressButtonAC);
    }

    void UpdateEngine(float normalizedSpeed)
    {
        float pitch = 0.4f + normalizedSpeed * 1f;
        engineSource.pitch = pitch;
    }

    void UpdateRadio(bool started)
    {
        if (started) 
        {
            if (musicSource.isPlaying)
            {
                readRadioInput = StartCoroutine(ReadRadioInput());
            }
            else
            {
                musicSource.Play();
            }        
        }
        else if (readingRadioInput)
        {
            StopCoroutine(readRadioInput);
            readingRadioInput = false;
            musicSource.clip = audioData.SwapMusicClip();
            musicSource.Play();
            UpdateRadioLabel();
        }
    }

    void UpdateRadioLabel()
    {
        radio_TMP.text = "Radio: " + musicSource.clip.name;
    }

    IEnumerator ReadRadioInput()
    {
        readingRadioInput = true;
        yield return new WaitForSecondsRealtime(1);
        readingRadioInput = false;
        musicSource.Pause();
    }
}
