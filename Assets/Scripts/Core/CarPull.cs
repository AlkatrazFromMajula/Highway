using ScriptableObjects;
using System.Collections;
using UnityEngine;

public class CarPull : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] AICarSpawnManagerSO spawnManager;


    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;
        bounds.center += transform.up * bounds.size.y;
        if (spawnManager != null)
            StartCoroutine(SpawnNonStop(bounds, renderer));
    }

    IEnumerator SpawnNonStop(Bounds bounds, Renderer renderer)
    {
        while (true) 
        {
            bool random = Random.Range(0, 3) == 0;
            if (random && Physics.Raycast(transform.position + transform.forward + transform.up, -transform.up, 2, 1 << 6) &&
                !Physics.CheckBox(bounds.center, bounds.size, transform.rotation, 1 << 10) && !renderer.isVisible)
            {
                spawnManager.SpawnRandomAICar(transform.position, transform.rotation);
            }
                
            yield return new WaitForSecondsRealtime(Random.Range(2, 5));
        }
    }
}
