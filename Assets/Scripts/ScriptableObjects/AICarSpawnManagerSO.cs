using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AI Car Spawn Manager", menuName = "Manager/AI Car Spawn Manager")]
    public class AICarSpawnManagerSO : ScriptableObject
    {
        GameObject[] cars;

        void OnEnable()
        {
            cars = Resources.LoadAll<GameObject>("Prefabs/AI_Cars");
        }

        public void SpawnRandomAICar(Vector3 position, Quaternion rotation)
        {
            int randomCar = Random.Range(0, cars.Length);
            Instantiate(cars[randomCar], position, rotation);
        }
    }
}
