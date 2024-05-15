using UnityEngine;

public class WorldKnot : MonoBehaviour
{
    [SerializeField] KnotType knotType;
    public enum KnotType { Straight, Bend }
}
