using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    protected IEnumerator<WaitForSecondsRealtime> SelfDestruct()
    {
        yield return new WaitForSecondsRealtime(5);
        Destroy(transform.parent.gameObject);
    }

    private void OnBecameInvisible()
    {
        if (gameObject.activeInHierarchy) { StartCoroutine(SelfDestruct()); }
    }

    private void OnBecameVisible()
    {
        StopAllCoroutines();
    }
}
