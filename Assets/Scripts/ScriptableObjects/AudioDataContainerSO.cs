using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Audio Data Container", menuName = "Data Container/Audio Data Container")]
    public class AudioDataContainerSO : ScriptableObject
    {
        [Header("Audio Clips")]
        [SerializeField] AudioClip collectCoinAC;
        [SerializeField] AudioClip pressButtonAC;
        [SerializeField] AudioClip startEngineAC;
        [SerializeField] AudioClip driveEngineAC;
        [SerializeField] AudioClip[] musicACs;

        public AudioClip CollectCoinAC => collectCoinAC;
        public AudioClip PressButtonAC => pressButtonAC;
        public AudioClip StartEngineAC => startEngineAC;
        public AudioClip DriveEngineAC => driveEngineAC;

        int selectedMusicClip;

        public AudioClip SwapMusicClip()
        {
            selectedMusicClip = (selectedMusicClip + 1) % 3;
            return musicACs[selectedMusicClip];
        }

        public AudioClip GetSelectedMusicClip()
        {
            return musicACs[selectedMusicClip];
        }
    }
}
