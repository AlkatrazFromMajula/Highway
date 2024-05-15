using System.Collections.Generic;
using UnityEngine;

public class SplineBuilderTool : MonoBehaviour
{
    public enum Shape { Curved, Circle, Straight }
    public Shape shape;
    public Vector3 offset;
    public List<float> lanes;
    public List<float> curvatureDif;
    public int quater;
    public int length;
}
