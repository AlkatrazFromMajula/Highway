using UnityEngine.Events;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Event/Bool Event Channel")]
    public class BoolEventChannelSO : ScriptableObject
    {
        public event UnityAction<bool> OnEventRaised;

        public void RaiseEvent(bool value)
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
