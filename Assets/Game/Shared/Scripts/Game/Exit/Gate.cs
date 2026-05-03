using UnityEngine;
using reign;
using System.Collections;

namespace FBH
{
    [RequireComponent(typeof(SoundPlaybacker))]
    public class Gate : MonoBehaviour
    {
        bool bool_Lifted;
        SoundPlaybacker SoundPlaybacker_Gate;
        [SerializeField] bool bool_LiftOnQuotaMet;
        [SerializeField] GameObject GameObject_Gate;
        [SerializeField] float float_TargetHeight = 5f;
        [SerializeField] InteractableSwitch InteractableSwitch_DoorSwitch;

        void OnEnable()
        {
            if (bool_LiftOnQuotaMet)
            {
                RuntimeUtility.Action_MetQuota += LiftGateNonEnumerator;
            }
        }
        void OnDisable()
        {
            StopAllCoroutines();
            RuntimeUtility.Action_MetQuota -= LiftGateNonEnumerator;
        }
        void Awake()
        {
            SoundPlaybacker_Gate = GetComponent<SoundPlaybacker>();
        }
        void Start()
        {
            bool_Lifted = false;
            GameObject_Gate.transform.localPosition = new(0, 0, 0);
            
            if(InteractableSwitch_DoorSwitch != null)
            {   
                InteractableSwitch_DoorSwitch.float_OnTime = 0f;
                InteractableSwitch_DoorSwitch.UnityEvent_OnSwitchedOn?.AddListener(()=>StartCoroutine(LiftGate()));   
            }
        }
        public void LiftGateNonEnumerator() => StartCoroutine(LiftGate());
        public IEnumerator LiftGate()
        {
            if(bool_Lifted) yield return null;

            Vector3 Vector3_Start = GameObject_Gate.transform.localPosition;
            Vector3 Vector3_Target = new(Vector3_Start.x, float_TargetHeight, Vector3_Start.z);

            float float_Time = 0;

            SoundPlaybacker_Gate?.Play("open_gate", 0.5f, false, Random.Range(0.89f, 1.12f));

            while (float_Time < 5f)
            {
                float_Time += RuntimeUtility.float_DeltaTime;
                GameObject_Gate.transform.localPosition = Vector3.Lerp(Vector3_Start, Vector3_Target, float_Time / 5f);
                yield return null;
            }

            GameObject_Gate.transform.localPosition = Vector3_Target;
        }
    }
}
