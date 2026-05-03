using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using reign;
using UnityEngine;
using UnityEngine.AI;

namespace FBH
{

    [RequireComponent(typeof(NavMeshAgent), typeof(AudioSource))]
    public class Enemy : MonoBehaviour
    {
        public static System.Action Action_OnEnemyActivated;
        public static System.Action<Transform> Action_OnHear;
        public static System.Func<EnemyDataSO> Action_GetEnemyData;
        protected AudioSource AudioSource_Ambience;
        public bool bool_HasLastKnown;
        Vector3 Vector3_LastKnown;
        protected float float_FOV = 80f, float_ViewDistance = 20f;
        [SerializeField] protected Transform Transform_Target;
        [SerializeField] protected LayerMask LayerMask_Ignore;
        protected NavMeshAgent NavMeshAgent_Agent;
        protected bool bool_IsChasing = true;
        protected float float_ChaseTime = 0f, float_Distance;
        public EnemyDataSO EnemyDataSO_Data;
        [SerializeField] List<Transform> Transform_Destinations = new();
        [SerializeField] GameObject GameObject_ExposureSphere;
        
        protected bool SeesPlayer()
        {
            if (Transform_Target == null) return false;
            
            Vector3 Vector3_Direction = (Transform_Target.position - transform.position).normalized;

            Debug.DrawRay(transform.position, transform.forward * float_ViewDistance, Color.red);
            if (float_Distance < 5f || Vector3.Angle(transform.forward, Vector3_Direction) < float_FOV)
            {
                if (Physics.Raycast(transform.position, Vector3_Direction, out RaycastHit HIT, float_ViewDistance, LayerMask_Ignore))
                {
                    return HIT.transform == Transform_Target;
                }
            }
            
            return false;
        }
        void OnTriggerEnter(Collider COLLIDER)
        {
            if(!COLLIDER.gameObject.TryGetComponent<InteractableDoor>(out InteractableDoor DOOR)) return;

            DOOR.Use();
            if(EnemyDataSO_Data.bool_CanBreakDoors && DOOR.bool_Locked)
            {
                DOOR.BreakOpen();
            }  
        }
        void OnEnable()
        {
            Action_GetEnemyData += GetEnemyData;
            Action_OnHear += SetTarget;
            OriginSystem.Action_OnUpdate += Frame;
        
            Action_OnEnemyActivated?.Invoke();
        }
        void OnDisable()
        {
            Action_GetEnemyData -= GetEnemyData;
            Action_OnHear -= SetTarget;
            OriginSystem.Action_OnUpdate -= Frame;
        }
        EnemyDataSO GetEnemyData()
        {
            return EnemyDataSO_Data;
        }
        void Awake()
        {
            NavMeshAgent_Agent = GetComponent<NavMeshAgent>();
            NavMeshAgent_Agent.updateRotation = true;

            if(EnemyDataSO_Data == null)
            {
                EnemyDataSO_Data = ScriptableObject.CreateInstance<EnemyDataSO>();
                EnemyDataSO_Data.float_Damage = Random.Range(1, 61);
                EnemyDataSO_Data.float_MovementSpeed = Random.Range(1, 11);
            }
            else
            {
                float_FOV = EnemyDataSO_Data.float_FOV;
                float_ViewDistance = EnemyDataSO_Data.float_ViewDistance;
            }
            
            AudioSource_Ambience = GetComponent<AudioSource>();
            AudioSource_Ambience.clip = EnemyDataSO_Data.AudioClip_AmbientSound;
            AudioSource_Ambience?.Play();
            
            float float_Cached = EnemyDataSO_Data.float_SphereScale;
            GameObject_ExposureSphere.transform.localScale = new(float_Cached, float_Cached, float_Cached);
            
            NavMeshAgent_Agent.angularSpeed = 270f; 
            SetRandomDestination();
        }
        void Frame()
        {
            if (Transform_Target == null) return;

            float_Distance = (transform.position - Transform_Target.position).sqrMagnitude;
            NavMeshAgent_Agent.speed = EnemyDataSO_Data.float_MovementSpeed * RuntimeUtility.float_GameTimeScale;

            if (SeesPlayer())
            {
                bool_IsChasing = true;
                Vector3_LastKnown = Transform_Target.position;
                bool_HasLastKnown = true;
                if (NavMeshAgent_Agent.destination != Transform_Target.position)
                {
                    NavMeshAgent_Agent.SetDestination(Transform_Target.position);
                }
            }
            else
            {
                bool_IsChasing = false;
                if(bool_HasLastKnown)
                {
                    NavMeshAgent_Agent.SetDestination(Vector3_LastKnown);
                    if (!NavMeshAgent_Agent.pathPending && NavMeshAgent_Agent.remainingDistance <= NavMeshAgent_Agent.stoppingDistance)
                    {
                        bool_HasLastKnown = false;
                    }
                }
                else
                {
                    if (!NavMeshAgent_Agent.pathPending && NavMeshAgent_Agent.remainingDistance <= NavMeshAgent_Agent.stoppingDistance)
                    {
                        SetRandomDestination();
                    }
                }
            }
        }
        void SetRandomDestination()
        {
            if(Transform_Destinations.Count <= 0) 
            {
                reign.Logger.Instance.Log(reign.Logger.enum_LogIntensity.Warning, "Enemy.cs: No destinations present");
                return;
            } 
            Vector3 Vector3_RandomPosition = Transform_Destinations[Random.Range(0, Transform_Destinations.Count)].position;
            if (NavMesh.SamplePosition(Vector3_RandomPosition, out NavMeshHit HIT, 3f, NavMesh.AllAreas))
            {
                NavMeshAgent_Agent.SetDestination(HIT.position);
            }
        }

        void SetTarget(Transform LOCATION)
        {
            NavMeshAgent_Agent.SetDestination(LOCATION.position);
        }
    }
}
