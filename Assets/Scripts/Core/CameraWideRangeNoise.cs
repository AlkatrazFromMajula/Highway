using Cinemachine;
using ScriptableObjects;
using System.Collections;
using UnityEngine;

public class CameraWideRangeNoise : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] VoidEventChannelSO beginGameEventChannel;

    [Header("Data Containers")]
    [SerializeField] CameraNoiseDataContainerSO noiseData;

    CinemachineTransposer transposer;
    Transform playerTransform;
    Vector3 targetOffset;
    Vector3 currVelocity;
    bool beginGame;

    void OnEnable() 
    {
        if (beginGameEventChannel != null)
            beginGameEventChannel.OnEventRaised += BeginGame;

        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>();
        if (noiseData != null)
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
            GetComponent<CinemachineVirtualCamera>().Follow = playerTransform;
            GetComponent<CinemachineVirtualCamera>().LookAt = playerTransform;
            transposer.m_FollowOffset = noiseData.InitialBodyOffset;
        }
        else
            Debug.LogWarning("Noise data not set");
    }

    void OnDisable()
    {
        if (beginGameEventChannel != null)
            beginGameEventChannel.OnEventRaised -= BeginGame;
    }

    void BeginGame()
    {
        StartCoroutine(BeginGameWithDelay());
    }

    IEnumerator BeginGameWithDelay()
    {
        yield return new WaitForEndOfFrame();
        targetOffset = noiseData.FirstTargetOffset;
        playerTransform = GameObject.FindWithTag("Player").transform;
        GetComponent<CinemachineVirtualCamera>().Follow = playerTransform;
        GetComponent<CinemachineVirtualCamera>().LookAt = playerTransform;
        beginGame = true;
    }

    void Update() 
    {
        if (beginGame)
        {
            if ((targetOffset - transposer.m_FollowOffset).magnitude > 1f) 
                transposer.m_FollowOffset = Vector3.SmoothDamp(transposer.m_FollowOffset, targetOffset, ref currVelocity, 5); 
            else 
                RandomizeTarget(); 
        }
    }

    void RandomizeTarget()
    {
        float randAngle = Random.Range(Mathf.PI, 2 * Mathf.PI);
        float randRadius = Random.Range(noiseData.InnerOuterRadius.x, noiseData.InnerOuterRadius.y);
        targetOffset.x = Mathf.Cos(randAngle) * randRadius;
        targetOffset.z = Mathf.Sin(randAngle) * randRadius;
        targetOffset.y = Random.Range(noiseData.MinMaxHight.x, noiseData.MinMaxHight.y);
    }
}