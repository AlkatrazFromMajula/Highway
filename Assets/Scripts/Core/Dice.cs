using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] int matchPriority = 1;
    [SerializeField] DiceFace[] faces;

    public int MatchPriority => matchPriority;
    public DiceFace[] Faces => faces;

    public void SpawnDice(DiceFace faceToMatch)
    {
        List<DiceFace> matchingFaces = new List<DiceFace>();
        foreach (DiceFace face in faces)
            if (faceToMatch.Compare(face)) { matchingFaces.Add(face); }

        int random = Random.Range(0, matchingFaces.Count);
        DiceFace randomFace = matchingFaces[random];
        //randomFace.active = false;
        FitDice(faceToMatch.transform, randomFace.transform);
        //randomFace.active = true;
    }

    void FitDice(Transform faceToMatch, Transform face)
    {
        float angle = Vector3.SignedAngle(face.forward, -faceToMatch.forward, Vector3.up);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        transform.position = faceToMatch.position - face.position;
        Instantiate(gameObject);
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
    }
}
