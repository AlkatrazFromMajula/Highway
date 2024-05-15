using System.Collections;
using UnityEngine;

public class PlayerIllumination : MonoBehaviour
{
    Transform camTransform;
    Transform playerTransform;

    void OnEnable()
    {
        camTransform = Camera.main.transform;
        StartCoroutine(IlluminatePlayer());      
    }

    IEnumerator IlluminatePlayer()
    {
        while (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
            yield return new WaitForEndOfFrame();
        }

        while (true) 
        {
            try
            {
                transform.rotation = Quaternion.LookRotation(playerTransform.position - camTransform.position);
            }
            catch (MissingReferenceException) 
            {
                playerTransform = GameObject.FindWithTag("Player").transform;
                transform.rotation = Quaternion.LookRotation(playerTransform.position - camTransform.position);
            }
            yield return new WaitForSeconds(0.2f);
        }  
    }
}
