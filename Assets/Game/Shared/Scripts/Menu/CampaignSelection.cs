using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using reign;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FBH
{
    public class CampaignSelection : MonoBehaviour, IDataHandler
    {
        bool bool_CampaignScreen;
        
        [Header("Campaign Choice")]
        CampaignSO CampaignSO_CurrentCampaign;
        [SerializeField] TMP_Text TMP_Text_Title;
        [SerializeField] TMP_Text TMP_Text_ChapterText;
        [SerializeField] Image Image_ChapterImage;
        [SerializeField] Button Button_PlayButton;
        [SerializeField] Button Button_NextButton;
        [SerializeField] int int_CurrentIndex; 

        [Header("Selection")]
        [SerializeField] GameObject[] GameObject_Screens;
        [SerializeField] float float_MaxY;
        [SerializeField] float float_MaxX; 
        [SerializeField] Camera Camera_FollowCam;
        public List<LevelScore> List_LevelScores;
        private Vector2 Vector2_Rotation, Vector2_Velocity, Vector2_Target;
        void OnEnable()
        {
            OriginSystem.Action_OnUpdate += Frame;
        }
        void OnDisable()
        {
            OriginSystem.Action_OnUpdate -= Frame;
        }
        void Start()
        {
            MasterSystem.Instance._DiscordSystem.SetTimestamp(App.Instance.long_AppUnixTimestamp);
            MasterSystem.Instance._DiscordSystem.SetState($"In the Menus");
            MasterSystem.Instance._DiscordSystem.SetDescription("");
            bool_CampaignScreen = false;
            LoadScreen(0);
        }
        void Frame()
        {
            if(!bool_CampaignScreen)
            {
                if(InputSystem.GetInput("Enter", InputSystem.enum_KeyType.Down, true))
                {
                    bool_CampaignScreen = true;
                    LoadScreen(1);
                }
            }


            Vector2 Vector2_MouseDelta = InputSystem.GetMouseDeltaVector();

            Vector2_Target += new Vector2(
                -Vector2_MouseDelta.y * float_MaxY * reign.Time.float_DeltaTime * 4f,
                Vector2_MouseDelta.x * float_MaxX * reign.Time.float_DeltaTime * 4f
            );

            Vector2_Target.x = Mathf.Clamp(Vector2_Target.x, -float_MaxY, float_MaxY);
            Vector2_Target.y = Mathf.Clamp(Vector2_Target.y, -float_MaxX, float_MaxX);

            Vector2_Rotation = Vector2.SmoothDamp(Vector2_Rotation, Vector2_Target, ref Vector2_Velocity, 0.4f);

            Camera_FollowCam.transform.localRotation = Quaternion.Euler(Vector2_Rotation.x, Vector2_Rotation.y, 0f);

            if(InputSystem.GetInput("Escape", InputSystem.enum_KeyType.Down, true))
            {
                App.Instance.HangApplication();
            }
        }
        public void LoadCampaign(CampaignSO CAMPAIGN)
        {
            if(CAMPAIGN.ChapterSO_Chapters.Count <= 0)
            {
                return;
            }

            CampaignSO_CurrentCampaign = CAMPAIGN;
            LoadScreen(2);
            Campaign_SetPage(0, CAMPAIGN);
            Button_NextButton.gameObject.SetActive(CampaignSO_CurrentCampaign.ChapterSO_Chapters.Count >= 2);
        }
        public void Campaign_NextPage()
        {
            if(int_CurrentIndex >= CampaignSO_CurrentCampaign.ChapterSO_Chapters.Count - 1)
            {
                Campaign_SetPage(0, CampaignSO_CurrentCampaign);
            }
            else
            {
                Campaign_SetPage(int_CurrentIndex + 1, CampaignSO_CurrentCampaign);
            }
        }
        void Campaign_SetPage(int PAGE, CampaignSO CAMPAIGN)
        {
            int_CurrentIndex = PAGE;
            Image_ChapterImage.sprite = CAMPAIGN.ChapterSO_Chapters[PAGE].Sprite_ChapterPreview;
            
            string string_Time = "No time set";

            foreach(LevelScore LEVELSCORE in List_LevelScores)
            {
                if(LEVELSCORE.string_ID == $"{CAMPAIGN.Enum_Campaign}_{CAMPAIGN.ChapterSO_Chapters[PAGE].int_ChapterNumber}")
                {
                    if(LEVELSCORE.float_CompletionTime != Mathf.Infinity) string_Time = reign.Time.ConvertToMMSS((int)LEVELSCORE.float_CompletionTime);
                }
            }    

            TMP_Text_ChapterText.text = $"Chapter {CAMPAIGN.ChapterSO_Chapters[PAGE].int_ChapterNumber} : {CAMPAIGN.ChapterSO_Chapters[PAGE].string_ChapterTitle}\n<size=90%>BEST TIME: {string_Time}";
            TMP_Text_Title.text = $"{CAMPAIGN.string_LongName}";
            
            Button_PlayButton.onClick.RemoveAllListeners();
            Button_PlayButton.onClick.AddListener(()=> LoadMap());
        }
        void LoadMap()
        {
            if(CampaignSO_CurrentCampaign.ChapterSO_Chapters[int_CurrentIndex].string_Scene == null) return;

            StaticDecisions.ChapterSO_Current = CampaignSO_CurrentCampaign.ChapterSO_Chapters[int_CurrentIndex];
            LoadingSystem.Action_TryLoad?.Invoke(CampaignSO_CurrentCampaign.ChapterSO_Chapters[int_CurrentIndex].string_Scene, 1, 1);
        }
        public void LoadScreen(int INDEX)
        {
            for (int i = 0; i < GameObject_Screens.Length; ++i)
            {
                if (i != INDEX)
                {
                    GameObject_Screens[i].SetActive(false);    
                }
                else
                {
                    GameObject_Screens[i].SetActive(true);
                }
            }
        }

        public void LoadData(GameData DATA)
        {
            RuntimeUtility.SetGraphicsQuality((int)DATA.enum_GraphicsQuality_Game, Camera_FollowCam);
            
            List_LevelScores = DATA.List_LevelScores;
        }

        public void SaveData(ref GameData DATA)
        {
        }
    }
}
