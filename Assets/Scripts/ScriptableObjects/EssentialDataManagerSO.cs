using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Essential Data Manager", menuName = "Manager/Essential Data Manager")]
    public class EssentialDataManagerSO : ScriptableObject
    {
        [Header("Data Containers")]
        [SerializeField] PlayableCarDataContainerSO[] playableCars;

        [Header("Playable Car Initial Params")]
        [SerializeField] Vector3 playableCarStartPosition;
        [SerializeField] Quaternion playableCarStartRotation;

        public PlayableCarDataContainerSO[] PlayableCars => playableCars;
        public Vector3 PlayableCarStartPosition => playableCarStartPosition;
        public Quaternion PlayableCarStartRotation => playableCarStartRotation;
        public int SelectedCar { get; set; }
        public int Budget { get; set; }

        GameObject carInstance;

        public PlayableCarDataContainerSO GetSelectedCar()
        {
            if (SelectedCar >= playableCars.Length || playableCars[SelectedCar] == null)
            {
                Debug.LogWarning(name + " playable car not set");
                return null;
            }   
            else
            {
                return playableCars[SelectedCar];
            }
        }

        public bool TryBuySelectedCar()
        {
            if (playableCars[SelectedCar].Price <= Budget)
            {
                Budget -= playableCars[SelectedCar].Price;
                playableCars[SelectedCar].IsBought = true;
                return true;
            }
            return false;
        }

        public bool TrySwapSelectedCar(int direction)
        {
            if (SelectedCar + direction >= 0 && SelectedCar + direction < playableCars.Length) 
            {
                SelectedCar += direction;
                return true;
            }
            return false;
        }

        public bool TrySpawnSelectedCar(bool original)
        {
            if (carInstance != null) 
            {
                Destroy(carInstance); 
            }
            return playableCars[SelectedCar].TrySpawn(out carInstance, playableCarStartPosition, playableCarStartRotation, original);
        }
    }
}
