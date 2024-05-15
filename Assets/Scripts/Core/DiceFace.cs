using ScriptableObjects;
using UnityEngine;

public class DiceFace : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] RoadConstructionManagerSO roadConstManager;

    public enum Width { Four, Six, Eight }
    public enum Classification { Highway, Avenue }

    [SerializeField] private Width numberOfLanes;
    public Width NumberOfLanes => numberOfLanes;

    [SerializeField] private Classification type;
    public Classification Type => type;

    [SerializeField] private bool isElivated;
    public bool IsElivated => isElivated;

    // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////
    //Bounds bounds;
    //Camera cam;
    //public bool active = true;
    // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 5);

        // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////
        //bounds = GetComponent<Renderer>().bounds;
        //cam = Camera.main;
        // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////
    }

    // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////
    //private void Update()
    //{
    //    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
    //    Vector3 boxHallfExtents = new Vector3(4, 0.5f, 0.5f);
    //    if (!Physics.CheckBox(transform.position + transform.forward * 1.5f, boxHallfExtents, transform.rotation, 1 << 6) && GeometryUtility.TestPlanesAABB(planes, bounds))
    //    {
    //        roadConstManager.FindMatch(this);
    //        active = false;
    //    }
    //}
    // EXPERIMENT //////////////////////////////////////////////////////////////////////////////////////

    public bool Compare(DiceFace other)
    {
        if (other == null) return false;
        return type == other.Type && numberOfLanes == other.NumberOfLanes && isElivated == other.IsElivated;
    }

    void OnBecameVisible()
    {
        Vector3 boxHallfExtents = new Vector3(4, 0.5f, 0.5f);
        if (!Physics.CheckBox(transform.position + transform.forward * 1.5f, boxHallfExtents, transform.rotation, 1 << 6))
        {
            roadConstManager.FindMatch(this);
        }
    }
}
