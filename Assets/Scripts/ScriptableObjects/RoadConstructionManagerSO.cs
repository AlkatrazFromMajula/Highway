using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Manager/Road Construction Manager")]
    public class RoadConstructionManagerSO : ScriptableObject
    {
        GameObject[] dices;

        void OnEnable()
        {
            dices = Resources.LoadAll<GameObject>("Prefabs/Chosen");
        }

        public void FindMatch(DiceFace faceToMatch)
        {
            List<Dice> matchingDices = new List<Dice>();
            int priorityQueue = 0;

            foreach (GameObject diceObj in dices)
            {
                Dice dice = diceObj.GetComponent<Dice>();

                foreach (DiceFace face in dice.Faces)
                    if (faceToMatch.Compare(face))
                    {
                        matchingDices.Add(dice);
                        priorityQueue += dice.MatchPriority;
                        break;
                    }
            }

            int random = Random.Range(0, priorityQueue);
            int queueInd = 0;

            foreach (Dice dice in matchingDices)
            {
                if (random >= queueInd && random < queueInd + dice.MatchPriority)
                {
                    if (dice.gameObject.name + "(Clone)" != faceToMatch.transform.parent.name)
                    {
                        dice.SpawnDice(faceToMatch);
                    }
                    else 
                    {
                        matchingDices.Remove(dice);
                        priorityQueue -= dice.MatchPriority;
                    }
                    break;
                }
                else { queueInd += dice.MatchPriority; }
            }
        }
    }
}
