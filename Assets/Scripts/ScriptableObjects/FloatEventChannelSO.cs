using UnityEngine.Events;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Event/Float Event Channel")]
    public class FloatEventChannelSO : ScriptableObject
    {
        public event UnityAction<float> OnEventRaised;

        public void RaiseEvent(float value)
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


