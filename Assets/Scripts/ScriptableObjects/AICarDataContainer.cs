using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Data Container/AI Car Data Container")]
    public class AICarDataContainer : ScriptableObject
    {
        [SerializeField] float acceleration;
        [SerializeField] float deceleration;
        [SerializeField] Vector2 minMax_MaxSpeed;

        public float Deceleration => deceleration;
        public float Acceleration => acceleration;
        public Vector2 MinMax_MaxSpeed => minMax_MaxSpeed;
    }
}
