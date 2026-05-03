using System;
using System.Collections;
using reign;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FBH
{
    public class RuntimeUtility : MonoBehaviour, IDataHandler
    {
        public static bool bool_GamePaused = false, bool_QuotaMet, bool_GameFinished;
        public static Action Action_FinishLevel;
        public static Action Action_MetQuota;
        public static Action<int> Action_ChangeGraphicsQuality;
        public static Action<int> Action_SetCollectableAmount;
        public static Action<bool> Action_PauseGame;
        public static Action<int> Action_AddCollectable;
        public static float float_GameTime = 0f;
        public static float float_GameTimeScale = 1f;
        public static float float_DeltaTime => UnityEngine.Time.deltaTime * float_GameTimeScale;
        public static int int_CollectableAmount {get; private set;} = 0;
        public ChapterSO ChapterSO_Default;
        void SetGraphicsQuality(int INDEX)
        {
            SetGraphicsQuality(INDEX, null);
        }

        [ContextMenu("Instacomplete")]
        public void Instacomplete()
        {
            if(App.Instance.AppData_App.bool_DebugBuild)
            {
                SetCollectableAmount(StaticDecisions.ChapterSO_Current.int_CollectableAmount);
            }            
        }
        public static void SetGraphicsQuality(int INDEX, Camera REFERENCE = null)
        {
            UniversalRenderPipelineAsset UniversalRenderPipelineAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            UniversalAdditionalCameraData URP = null;

            if(REFERENCE == null)
            {
                URP = Camera.allCameras[0].GetComponent<UniversalAdditionalCameraData>(); 
            }
            else
            {
                URP = REFERENCE.GetComponent<UniversalAdditionalCameraData>();
            }

            switch (INDEX)
            {
                case 0:
                    {
                        // Low
                        UniversalRenderPipelineAsset.renderScale = 0.7f;
                        UniversalRenderPipelineAsset.shadowDistance = 0;
                        URP.renderShadows = false;
                        URP.renderPostProcessing = false;
                        URP.antialiasing = AntialiasingMode.None;
                        break;
                    }
                case 1:
                    {
                        // Medium
                        UniversalRenderPipelineAsset.renderScale = 0.85f;
                        UniversalRenderPipelineAsset.shadowDistance = 15;
                        URP.renderShadows = true;
                        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
                        URP.renderPostProcessing = true;
                        URP.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                        URP.antialiasingQuality = AntialiasingQuality.Low;
                        break;
                    }
                case 2:
                    {
                        // High
                        UniversalRenderPipelineAsset.renderScale = 1.0f;
                        UniversalRenderPipelineAsset.shadowDistance = 40;
                        URP.renderShadows = true;
                        URP.renderPostProcessing = true;
                        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
                        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                        URP.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        URP.antialiasingQuality = AntialiasingQuality.High;
                        break;
                    }
            }
        }
        public static IEnumerator WaitForGameSeconds(float SECONDS)
        {
            float float_T = 0f;

            while (float_T < SECONDS)
            {
                float_T += float_DeltaTime;
                yield return null;
            }
        }
        void OnEnable()
        {
            Action_ChangeGraphicsQuality += SetGraphicsQuality;
            Action_FinishLevel += FinishGame;
            OriginSystem.Action_OnSecond += Lapse;
            Action_PauseGame += PauseGame;
            Action_MetQuota += MeetQuota;
            Action_AddCollectable += AddCollectable;
            Action_SetCollectableAmount += SetCollectableAmount;
        }
        void OnDisable()
        {
            Action_ChangeGraphicsQuality -= SetGraphicsQuality;
            Action_FinishLevel -= FinishGame;
            OriginSystem.Action_OnSecond -= Lapse;
            Action_PauseGame -= PauseGame;
            Action_MetQuota -= MeetQuota;
            Action_AddCollectable -= AddCollectable;
            Action_SetCollectableAmount -= SetCollectableAmount; 
        }
        void MeetQuota() => bool_QuotaMet = true;
        protected void PauseGame(bool PAUSED)
        {
            bool_GamePaused = PAUSED;
            float_GameTimeScale = PAUSED ? 0f : 1f;
        }
        protected void FinishGame()
        {
            bool_GameFinished = true;
            MasterSystem.Instance._SaveSystem.SaveGameData();
        }
        void Awake()
        {
            if(StaticDecisions.ChapterSO_Current == null)
            {
                StaticDecisions.ChapterSO_Current = ChapterSO_Default;
            }
        }
        void Start()
        {
            Action_PauseGame?.Invoke(false);
            SetCollectableAmount(0);
            bool_GameFinished = false;
            bool_GamePaused = false;
            float_GameTimeScale = 1f;
            float_GameTime = 0f;
            UnityEngine.Time.timeScale = 1f;

            PlayerUI.Action_Setup?.Invoke();
            MasterSystem.Instance._DiscordSystem.SetState($"Playing {StaticDecisions.ChapterSO_Current.CampaignSO_Related.Enum_Campaign} | Chapter {StaticDecisions.ChapterSO_Current.int_ChapterNumber}");
        }
        void Lapse()
        {
            if(bool_GamePaused || bool_GameFinished) return;
            float_GameTime += 1;
            PlayerUI.Action_SetTimeText?.Invoke(reign.Time.ConvertToMMSS((int)float_GameTime));
        }
        void ChangeGraphicsQuality(int INDEX)
        {
            Action_ChangeGraphicsQuality?.Invoke(INDEX);
        }

        public void AddCollectable(int AMT)
        {
            SetCollectableAmount(int_CollectableAmount + AMT);
        }

        void SetCollectableAmount(int AMT)
        {
            int_CollectableAmount = AMT;
            PlayerUI.Action_SetCollectableText?.Invoke(AMT);

            if(int_CollectableAmount == StaticDecisions.ChapterSO_Current.int_CollectableAmount)
            {
                Action_MetQuota?.Invoke();
            }
            
            MasterSystem.Instance._DiscordSystem.SetDescription($"{int_CollectableAmount}/{StaticDecisions.ChapterSO_Current.int_CollectableAmount} {StaticDecisions.ChapterSO_Current.string_CollectableName}s");
        }

        public void LoadData(GameData DATA)
        {
            ChangeGraphicsQuality((int)DATA.enum_GraphicsQuality_Game);
        }

        public void SaveData(ref GameData DATA)
        {
            if (!bool_GameFinished) return;

            string string_ID = $"{StaticDecisions.ChapterSO_Current.CampaignSO_Related.Enum_Campaign}_{StaticDecisions.ChapterSO_Current.int_ChapterNumber}";
            bool bool_Found = false;

            foreach (LevelScore LEVELSCORE in DATA.List_LevelScores)
            {
                if (LEVELSCORE.string_ID == string_ID)
                {
                    bool_Found = true;

                    if (float_GameTime < LEVELSCORE.float_CompletionTime)
                    {
                        LEVELSCORE.float_CompletionTime = float_GameTime;
                    }

                    break;
                }
            }

            if (!bool_Found)
            {
                DATA.List_LevelScores.Add(new LevelScore(string_ID, float_GameTime));
            }
        }
    }
}
