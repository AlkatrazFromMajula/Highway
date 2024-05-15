using UnityEngine.Events;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Event/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public event UnityAction<int> OnEventRaised;

        public void RaiseEvent(int value)
        {
            if (OnEventRaised != null)
            {
                OnEventRaised.Invoke(value);
            }
            else
            {
                Debug.LogWarning("An event was raised on " + name + " but nobody listened");
            }
        }
    }
}

