using ScriptableObjects;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class AI_ObstacleCar : MonoBehaviour
{
    [Header("Data Containers")]
    [SerializeField] AICarDataContainer carData;

    SplineAnimate splineAnim;
    GameObject splineHolder;
    Coroutine selfdestruction;

    float maxSpeed;
    public float currentSpeed;
    bool wasSeen;

    private void OnEnable()
    {
        splineAnim = GetComponent<SplineAnimate>();
        splineAnim.OnEndReached += SwapRoad;

        maxSpeed = UnityEngine.Random.Range(carData.MinMax_MaxSpeed.x, carData.MinMax_MaxSpeed.y);
        currentSpeed = maxSpeed;

        StartCoroutine(CheckForRoad());
        StartCoroutine(DelayedFirstRoadSwap());
    }

    IEnumerator DelayedFirstRoadSwap()
    {
        yield return new WaitForEndOfFrame();
        SwapRoad();
    }

    private void SwapRoad()
    {
        Ray roadTracker = new Ray(transform.position + transform.forward * 2 + transform.up, -transform.up);
        if (Physics.Raycast(roadTracker, out RaycastHit hitInfo, 2, 1<<6))
        {

            //try
            {
                if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "Road")
                {
                    if (splineHolder == null) { splineHolder = new GameObject("AI Spline Holder", typeof(SplineContainer)); }
                    splineHolder.transform.position = hitInfo.transform.parent.position;
                    splineHolder.transform.rotation = hitInfo.transform.parent.rotation;
                    hitInfo.transform.parent.GetComponentInChildren<SplineContainer>().MatchLanes(hitInfo.point, splineHolder.GetComponent<SplineContainer>());
                    splineAnim.Container = splineHolder.GetComponent<SplineContainer>();
                    splineAnim.Restart(false);
                }
            }
            //catch (NullReferenceException) { print("opa"); }
        }
        else { if (splineHolder != null) { Destroy(splineHolder); } Destroy(gameObject); }
    }

    void Update()
    {
        UpdateSplineAnimate();

        if (Physics.Raycast(transform.TransformPoint(new Vector3(0, 1, 0)), transform.forward, 20, (1<<10) | (1<<3)))
        {
            if (currentSpeed > 0) { currentSpeed = Mathf.Max(currentSpeed - carData.Deceleration * Time.deltaTime, 0); }
        }
        else if (currentSpeed < maxSpeed) { currentSpeed = Mathf.Min(currentSpeed + carData.Acceleration * Time.deltaTime, maxSpeed); }
    }

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

    IEnumerator StartSelfDestruction()
    {
        yield return new WaitForSecondsRealtime(5);
        Destroy(splineHolder);
        Destroy(gameObject);
    }

    IEnumerator CheckForRoad()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5);
            if (!Physics.Raycast(transform.position + transform.up, -transform.up, 2, 1 << 6)) { Destroy(splineHolder); Destroy(gameObject); }
        }
    }

    void OnBecameVisible()
    {
        wasSeen = true;      
        StopCoroutine(selfdestruction);
    }

    void OnBecameInvisible() 
    {
        if (wasSeen) { selfdestruction = StartCoroutine(StartSelfDestruction()); }
    }
}
