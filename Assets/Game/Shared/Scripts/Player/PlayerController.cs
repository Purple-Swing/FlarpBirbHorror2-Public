using System;
using System.Collections;
using System.Collections.Generic;
using reign;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FBH
{
    [RequireComponent(typeof(CharacterController), typeof(SoundPlaybacker))]
    public class PlayerController : MonoBehaviour, IDataHandler
    {
        public AudioSource AudioSource_Exposure;
        public static SoundPlaybacker SoundPlaybacker_PlayerSound;
        public static Action Action_Kill;
        public static Action<float> Action_AddStamina;
        public static Action<float> Action_SetFOV;
        public float float_BaseSpeed = 4f;
        public static float float_Stamina = 100f, float_Exposure = 0f, float_ExposureTimer = 0;
        float float_Speed = 4f;
        Color Color_Cached = new(1,1,1);
        [SerializeField] Image Image_ChaseOverlay;
        [SerializeField] protected Transform Transform_Feet, Transform_Birb;
        [SerializeField] Camera Camera_PlayerCamera; 
        [SerializeField] TMP_Text TMP_Text_ExposureText;
        protected bool bool_Grounded = true, bool_WasGrounded = true;
        protected const float float_MINMAXLOOK = 90f, float_CAMERASENS = 3.2f;
        protected CharacterController CharacterController_Player;
        protected float float_H, float_F, float_VerticalRotation;
        protected Vector2 Vector2_MouseDelta;
        protected Vector3 Vector3_Move, Vector3_Velocity; 
        protected bool bool_CanPause = false, bool_CanMove = true;
        protected float float_FootstepTimer = 0f, float_ExposureTickDamage, float_ExposureDistance;
        protected EnemyDataSO EnemyDataSO_Clone;
        const float float_RunFootstepInterval = 0.35f, float_FootstepInterval = 0.56f;
        void OnEnable()
        {
            Enemy.Action_OnEnemyActivated += OnActivateEnemy;
            Action_AddStamina += AddStamina;
            Action_SetFOV += SetFOV;
            OriginSystem.Action_OnUpdate += Frame;
            RuntimeUtility.Action_FinishLevel += FinishGame;
        }
        void OnDisable()
        {
            Enemy.Action_OnEnemyActivated -= OnActivateEnemy;
            Action_AddStamina -= AddStamina;
            Action_SetFOV -= SetFOV;
            OriginSystem.Action_OnUpdate -= Frame;
            RuntimeUtility.Action_FinishLevel -= FinishGame;
        }
        void AddStamina(float AMT)
        {
            float_Stamina += AMT;
        }
        public void SetFOV(float VALUE)
        {
            if(Camera_PlayerCamera == null) return;
            Camera_PlayerCamera.fieldOfView = VALUE;
        }
        void OnActivateEnemy()
        {
            EnemyDataSO DATA = Enemy.Action_GetEnemyData?.Invoke();
            float_ExposureTickDamage = DATA.float_Damage;
            float_ExposureDistance = 2 + ((DATA.float_SphereScale * 0.50f) * (DATA.float_SphereScale * 0.50f));
        }
        void Awake()
        {
            bool_CanMove = true;
            if(Transform_Birb == null) Transform_Birb = GameObject.FindGameObjectWithTag("Enemy").transform;

            Camera_PlayerCamera = GetComponentInChildren<Camera>();
            SoundPlaybacker_PlayerSound = GetComponent<SoundPlaybacker>();
            CharacterController_Player = GetComponent<CharacterController>();

        }
        void Start()
        {   
            TMP_Text_ExposureText.alpha = 0;   
            float_Exposure = 0.0f;
            float_Stamina = 100.0f;
            
            MasterSystem.Instance._CursorSystem.SetCursor("Arrow", CursorLockMode.Locked, false);
            StartCoroutine(PauseDelay());
        }
        IEnumerator PauseDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            bool_CanPause = true;
        }
        void FinishGame()
        {
            bool_CanPause = false;
            bool_CanMove = false;
        }
        void Frame()
        {
            if(Transform_Birb.gameObject.activeInHierarchy && bool_CanMove)
            {
                float float_Distance = (transform.position - Transform_Birb.position).sqrMagnitude;

                if(float_Distance < float_ExposureDistance)
                {
                    float_Exposure += float_ExposureTickDamage * RuntimeUtility.float_GameTimeScale * float_Distance / 10.0f;
                }
                else
                {
                    float_Exposure -= 2.0f * RuntimeUtility.float_GameTimeScale * float_Distance / 10.0f;
                }

                float_Exposure = Mathf.Clamp(float_Exposure, 0.0f, 100.0f);
                Color_Cached.a = float_Exposure * (float_ExposureTimer / 5) / 100f;
                Image_ChaseOverlay.color = Color_Cached;

                if(float_Exposure > 0.0f)
                {
                    TMP_Text_ExposureText.alpha = float_Exposure * (float_ExposureTimer / 5.0f) / 100.0f;
                    float_ExposureTimer += reign.Time.float_DeltaTime * RuntimeUtility.float_GameTimeScale;
                    if(!AudioSource_Exposure.isPlaying)
                    {
                        AudioSource_Exposure.Play();
                    }
                    AudioSource_Exposure.volume = float_Exposure * (float_ExposureTimer + 1.0f / 5.0f) / 50.0f;
                    AudioSource_Exposure.pitch = float_Exposure * (float_ExposureTimer + 1.0f / 2.5f) / 100.0f;    
                }
                else
                {
                    TMP_Text_ExposureText.alpha = 0;
                    AudioSource_Exposure.Stop();
                    float_ExposureTimer = 0.0f;
                }

                if(float_ExposureTimer > 5.0f)
                {
                    float_ExposureTimer = 0.0f;
                    SoundPlaybacker_PlayerSound.PlayOneShot("player_death", 2f);
                    LoadingSystem.Action_TryLoad?.Invoke(SceneManager.GetActiveScene().name, 0.0f, 0.0f);
                    Action_Kill?.Invoke();
                }
            }
            else
            {
                float_Exposure = 0.0f;
                Color_Cached.a = 0.0f;
                Image_ChaseOverlay.color = Color_Cached;
            }

            if(bool_CanPause && InputSystem.GetInput("Pause", InputSystem.enum_KeyType.Down))
            {
                RuntimeUtility.Action_PauseGame?.Invoke(!RuntimeUtility.bool_GamePaused);
            }

            if(RuntimeUtility.bool_GamePaused || !bool_CanMove) 
            {
                return;
            }

            Jump();
            Inputs();
            Move();
            Camera();
            Footsteps();
        }
        void Jump()
        {
            bool_Grounded = Physics.Raycast(Transform_Feet.position, Vector3.down, 0.3f) || CharacterController_Player.isGrounded;

            if (!bool_WasGrounded && bool_Grounded && CharacterController_Player.velocity.y <= 0.0f)
            {
                SoundPlaybacker_PlayerSound.PlayOneShot("player_land", 0.5f);
            }
            
            bool_WasGrounded = bool_Grounded;

            if (bool_Grounded && Vector3_Velocity.y < 0f) 
            {
                Vector3_Velocity.y = -0.3f;
            }

            if (bool_Grounded && InputSystem.GetInput("Jump", InputSystem.enum_KeyType.Down))
            { 
                SoundPlaybacker_PlayerSound.PlayOneShot("player_jump", 0.5f);
                Vector3_Velocity.y = 1.9f;
            }
            
            Vector3_Velocity.y += -3.5f * reign.Time.float_DeltaTime;
        }
        void Footsteps()
        {
            if (!bool_Grounded) return;

            if (float_H == 0 && float_F == 0) 
            {
                float_FootstepTimer = 0;
                return;
            }

            float float_Interval = InputSystem.GetInput("Run", InputSystem.enum_KeyType.Held) && float_Stamina > 0 ? float_RunFootstepInterval : float_FootstepInterval;
            float_FootstepTimer -= reign.Time.float_DeltaTime;

            if (float_FootstepTimer <= 0f)
            {
                SoundPlaybacker_PlayerSound.PlayOneShot($"player_footstep_{UnityEngine.Random.Range(1, 5)}", 0.4f);
                float_FootstepTimer = float_Interval;
            }
        }

        void Inputs()
        {
            if(InputSystem.GetInput("A", InputSystem.enum_KeyType.Held))
            {
                float_H = -1;
            }
            else if (InputSystem.GetInput("D", InputSystem.enum_KeyType.Held))
            {
                float_H = 1;
            }
            else
            {
                float_H = 0;
            }
            
            if (InputSystem.GetInput("W", InputSystem.enum_KeyType.Held))
            {
                float_F = 1;
            }
            else if (InputSystem.GetInput("S", InputSystem.enum_KeyType.Held))
            {
                float_F = -1;
            }
            else
            {
                float_F = 0;
            }

            if(float_H == 0 && float_F == 0)
            {
                float_Stamina += 20f * reign.Time.float_DeltaTime;                
            }

            if(InputSystem.GetInput("Run", InputSystem.enum_KeyType.Held) && float_Stamina > 0)
            {
                float_Speed = float_BaseSpeed * 1.6f;
                float_Stamina -= 13f * reign.Time.float_DeltaTime;
            }
            else
            {
                float_Speed = float_BaseSpeed;
            }
            float_Stamina = Mathf.Clamp(float_Stamina, 0, 100);
        }

        void Move()
        {
            Vector3_Move = transform.forward * float_F + transform.right * float_H;
            Vector3_Move.Normalize();
            
            Vector3 Vector3_Final = (Vector3_Move * float_Speed) + new Vector3(0,Vector3_Velocity.y*4,0);

            CharacterController_Player.Move(reign.Time.float_DeltaTime * Vector3_Final);
        } 
        void Camera()
        {
            if(Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2_MouseDelta = InputSystem.GetMouseDeltaVector();

            float_VerticalRotation -= Vector2_MouseDelta.y * float_CAMERASENS;
            float_VerticalRotation = Mathf.Clamp(float_VerticalRotation, -float_MINMAXLOOK, float_MINMAXLOOK);

            Camera_PlayerCamera.transform.localRotation = Quaternion.Euler(float_VerticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * Vector2_MouseDelta.x * float_CAMERASENS);
        }

        public void LoadData(GameData DATA)
        {
            RuntimeUtility.SetGraphicsQuality((int)DATA.enum_GraphicsQuality_Game, Camera_PlayerCamera);
            SetFOV(DATA.float_FOV);
        }

        public void SaveData(ref GameData DATA)
        {
        }
    }
}
