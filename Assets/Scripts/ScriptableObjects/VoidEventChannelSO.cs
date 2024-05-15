using UnityEngine.Events;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Event/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event UnityAction OnEventRaised;

        public void RaiseEvent()
        {
            if (OnEventRaised != null)
            {
                OnEventRaised.Invoke();
            }
            else
            {
                Debug.LogWarning("An event was raised on " + name + " but nobody listened");
            }
        }
    }
}
