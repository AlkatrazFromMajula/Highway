using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Data Container/Playable Car Data Container")]
    public class PlayableCarDataContainerSO : ScriptableObject
    {
        [SerializeField] GameObject originalPrefab;
        [SerializeField] GameObject blankPrefab;
        [SerializeField] float maxSpeed;
        [SerializeField] float acceleration;
        [SerializeField] float deceleration;
        [SerializeField] int price;
        public bool IsBought;
        float decelerationOverTime = 15;

        public float DecelerationOverTime => decelerationOverTime;
        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public int Price => price;
        public GameObject Prefab => originalPrefab;
        public GameObject BlankPrefab => blankPrefab;

        public bool TrySpawn(out GameObject carInstance, Vector3 position, Quaternion rotation, bool original)
        {
            GameObject prefab;
            if (original && originalPrefab != null)
            {
                prefab = originalPrefab;
            }
            else if (!original && blankPrefab != null)
            {
                prefab = blankPrefab;
            }
            else
            {
                Debug.LogWarning(name + " prefab not set");
                carInstance = null;
                return false;
            }

            carInstance = Instantiate(prefab, position, rotation);
            return true;
        }
    }
}
