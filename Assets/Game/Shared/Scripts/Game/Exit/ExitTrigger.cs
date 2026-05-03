using UnityEngine;

namespace FBH
{
    [RequireComponent(typeof(BoxCollider))]
    public class ExitTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider OTHER)
        {
            if(RuntimeUtility.bool_QuotaMet && OTHER.CompareTag("Player"))
            {
                RuntimeUtility.Action_FinishLevel?.Invoke();
            }
        }
    }
}
