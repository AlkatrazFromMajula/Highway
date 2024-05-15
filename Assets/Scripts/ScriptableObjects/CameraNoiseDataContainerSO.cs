using UnityEngine;

[CreateAssetMenu(fileName = "Camera Noise Data Container", menuName = "Data Container/Camera Noise Data Container")]
public class CameraNoiseDataContainerSO : ScriptableObject
{
    [Header("Noise Area Parameters")]
    [SerializeField] Vector3 initialBodyOffset;
    [SerializeField] Vector3 firstTargetOffset;
    [SerializeField] Vector2 innerOuterRadius;
    [SerializeField] Vector2 minMaxHight;

    public Vector3 InitialBodyOffset => initialBodyOffset;
    public Vector3 FirstTargetOffset => firstTargetOffset;
    public Vector3 InnerOuterRadius => innerOuterRadius;
    public Vector2 MinMaxHight => minMaxHight;
}
