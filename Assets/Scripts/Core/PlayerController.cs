using UnityEngine.Splines;
using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;
using ScriptableObjects;

public class PlayerController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] InputManagerSO inputManager;

    [Header("Data Containers")]
    [SerializeField] PlayableCarDataContainerSO carData;

    [Header("Event Channels")]
    [SerializeField] VoidEventChannelSO pickCoinEventChannel;
    [SerializeField] VoidEventChannelSO finishEventChannel;
    [SerializeField] FloatEventChannelSO playerSpeedChangeEventChannel;

    [Header("Turn Tension Test")]
    [SerializeField] Vector2 tensionLimits;
    [SerializeField] Material turnTest;

    [Header("Trackers")]
    [SerializeField] Vector2 minMaxTrackerOffset;

    Rigidbody rb;
    Animator animator;
    SplineAnimate splineAnim;
    GameObject splineHolder;

    BoxCollider[] trackers_straight;
    Rigidbody[] destructComponents;

    float tension;
    float currentSpeed;
    bool selfDestructing;
    bool changingLanes;
    bool isAccelerating;
    bool isDecelerating;
    int speedHash;

    void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnChangeLane += UpdateTracker;
            inputManager.OnDe_Accelerate += ReadDe_AccelerationInput;
        }
        
        splineAnim = GetComponent<SplineAnimate>();
        splineAnim.OnEndReached += SwapRoad;
        splineAnim.OnLanesChanged += LanesChanged;

        trackers_straight = GetComponentsInChildren<BoxCollider>(true);
        trackers_straight = new BoxCollider[] { trackers_straight[1], trackers_straight[2] };

        animator = GetComponent<Animator>();
        speedHash = Animator.StringToHash("speed");

        rb = GetComponent<Rigidbody>();
        destructComponents = GetComponentsInChildren<Rigidbody>();

        SwapRoad();
    }

    void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnChangeLane -= UpdateTracker;
            inputManager.OnDe_Accelerate -= ReadDe_AccelerationInput;
        }
    }

    void SwapRoad()
    {
        Ray roadTracker = new Ray(transform.position + transform.forward * 2 + transform.up, -transform.up);
        if (Physics.Raycast(roadTracker, out RaycastHit hitInfo, 2, 1 << 6))
        {
            try
            {
                if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "Road")
                {
                    if (splineHolder == null) { splineHolder = new GameObject("Player Spline Holder", typeof(SplineContainer)); }
                    splineHolder.transform.position = hitInfo.transform.parent.position;
                    splineHolder.transform.rotation = hitInfo.transform.parent.rotation;
                    hitInfo.transform.parent.GetComponentInChildren<SplineContainer>().MatchLanes(hitInfo.point, splineHolder.GetComponent<SplineContainer>());
                    splineAnim.Container = splineHolder.GetComponent<SplineContainer>();
                    splineAnim.Restart(false);
                }
            }
            catch (NullReferenceException) { }
        }
    }

    void UpdateTracker(int direction, bool isActive)
    {
        if (direction > 0)
            trackers_straight[0].enabled = isActive;
        else if (direction < 0)
            trackers_straight[1].enabled = isActive;
        else
            foreach (BoxCollider tracker in trackers_straight)
                tracker.enabled = false;
    }

    void TransformTrackers()
    {
        float projectedZ = minMaxTrackerOffset.x + (currentSpeed / carData.MaxSpeed) * (minMaxTrackerOffset.y - minMaxTrackerOffset.x);
        foreach (BoxCollider tracker in trackers_straight)
        {
            tracker.center = new Vector3(tracker.center.x, tracker.center.y, projectedZ);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!changingLanes)
        {
            float angle = Mathf.Abs(Vector3.SignedAngle(transform.forward, other.transform.forward, Vector3.up));
            if (((1 << 7) & (1 << other.gameObject.layer)) != 0 && (angle <= 30 || angle >= 150))
            {
                changingLanes = true;
                other.GetComponentInParent<SplineContainer>().SliceFromKnot(other.gameObject, transform, splineHolder.GetComponent<SplineContainer>());
                splineAnim.Container = splineHolder.GetComponent<SplineContainer>();
                splineAnim.Restart(false);
                StartCoroutine(splineAnim.LaneChangeCooldown());
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((((1 << 6) | (1 << 10)) & (1 << collision.gameObject.layer)) != 0 && !selfDestructing)
        {
            selfDestructing = true;
            Vector3 hitDirection = (transform.position - collision.GetContact(0).point);
            hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z).normalized;
            StartCoroutine(StartPlayerDestruction(hitDirection));
        }
        else if ((1 << 12 & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(collision.gameObject);
            pickCoinEventChannel.RaiseEvent();
        }
    }

    IEnumerator StartPlayerDestruction(Vector3 hitDirection)
    {
        Vector3 firstFramePos = transform.position;
        yield return new WaitForEndOfFrame();
        Vector3 secondFramePos = transform.position;
        float velocity = (secondFramePos - firstFramePos).magnitude / Time.deltaTime;

        splineAnim.enabled = false;
        animator.enabled = false;
        currentSpeed = 0;

        rb.useGravity = true;
        rb.AddForce(hitDirection * velocity / 10, ForceMode.Impulse);

        for (int i = 1; i < destructComponents.Length; i++)
        {
            destructComponents[i].GetComponent<Transform>().parent = null;
            destructComponents[i].AddComponent<BoxCollider>().includeLayers = 1 << 6;

            destructComponents[i].isKinematic = false;
            destructComponents[i].useGravity = true;

            Vector3 direction = rb.position - destructComponents[i].position;
            destructComponents[i].AddForce(direction.normalized * 10, ForceMode.Impulse);
        }
        finishEventChannel.RaiseEvent();
        enabled = false;
    }

    void LanesChanged() { changingLanes = false; }

    void UpdateSplineAnimate()
    {
        float elapsedTime = splineAnim.ElapsedTime + currentSpeed * Time.deltaTime;
        if (elapsedTime > splineAnim.Duration)
        {
            float lostTime = elapsedTime - splineAnim.Duration;
            splineAnim.ElapsedTime = splineAnim.Duration;
            splineAnim.Update();
            splineAnim.ElapsedTime += lostTime;
        }
        else
        {
            splineAnim.ElapsedTime = elapsedTime;
        }
    }

    void ReadDe_AccelerationInput(int direction, bool started)
    {
        if (direction > 0)
            isAccelerating = started;
        else if (direction < 0)
            isDecelerating = started;
        else
        {
            isAccelerating = false;
            isDecelerating = false;
        }
    }

    private void Update()
    {
        if ((isDecelerating || !isAccelerating) && currentSpeed > 0) { Decelerate(isDecelerating); }
        else if (isAccelerating && !isDecelerating && currentSpeed < carData.MaxSpeed) { Accelerate(); }

        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.RaiseEvent(currentSpeed / carData.MaxSpeed);

        animator.SetFloat(speedHash, currentSpeed / carData.MaxSpeed * 5);

        UpdateSplineAnimate();
        //HandleTurns();
    }

    void Decelerate(bool isDecelerating)
    {
        currentSpeed -= (isDecelerating ? carData.Deceleration : carData.DecelerationOverTime) * Time.deltaTime;
        if (currentSpeed < 0.005f) { currentSpeed = 0; }

        TransformTrackers();
    }
    
    void Accelerate()
    {
        currentSpeed += carData.Acceleration * Time.deltaTime;
        if (currentSpeed > carData.MaxSpeed - 0.005f) { currentSpeed = carData.MaxSpeed; }

        TransformTrackers();
    }

    //void HandleTurns()
    //{ 
    //    float curvature = splineAnim.Container.Spline.EvaluateCurvature(splineAnim.NormalizedTime);
    //    tension = currentSpeed * curvature;
    //    tension = Mathf.Round(tension * 100) / 100;
    //    if (tension >= 0 && tension < tensionLimits.x)
    //    {
    //        turnTest.color = Color.green;
    //    }
    //    else if (tension >= tensionLimits.x && tension < tensionLimits.y)
    //    {
    //        turnTest.color = Color.yellow;
    //    }
    //    else if (tension >= tensionLimits.y)
    //    {
    //        turnTest.color = Color.red;
    //    }
    //}
}
