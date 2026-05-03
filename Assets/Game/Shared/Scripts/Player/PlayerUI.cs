using System;
using System.Collections;
using System.Collections.Generic;
using reign;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FBH
{
    public class PlayerUI : MonoBehaviour
    {
        public static Action Action_Setup;
        public static Action<string> Action_SetTimeText;
        public static Action<Sprite> Action_SetCollectableUISprite;
        public static Action<int> Action_SetCollectableText;
        [SerializeField] GameObject GameObject_PauseScreen, GameObject_WinScreen, GameObject_MainScreen;
        [SerializeField] TMP_Text TMP_Text_Time, TMP_Text_EndTime;
        [SerializeField] TMP_Text TMP_Text_Collectable;
        [SerializeField] Image Image_Collectable;
        [SerializeField] Slider Slider_Stamina;
        
        void OnEnable()
        {
            OriginSystem.Action_OnUpdate += Frame;
            RuntimeUtility.Action_FinishLevel += FinishGame;
            RuntimeUtility.Action_PauseGame += PauseScreen;
            Action_SetCollectableUISprite += SetCollectableUISprite;
            Action_SetCollectableText += SetCollectableText;
            Action_SetTimeText += SetTimeText;
            Action_Setup += Setup;
        }

        void OnDisable()
        {
            OriginSystem.Action_OnUpdate -= Frame;
            RuntimeUtility.Action_FinishLevel -= FinishGame;
            RuntimeUtility.Action_PauseGame -= PauseScreen;
            Action_SetCollectableUISprite -= SetCollectableUISprite;
            Action_SetCollectableText -= SetCollectableText;
            Action_SetTimeText -= SetTimeText;
            Action_Setup -= Setup;
        }
        void Frame()
        {
            if(RuntimeUtility.bool_GamePaused) return;
            Slider_Stamina.value = PlayerController.float_Stamina;
        }

        void Setup()
        {
            if (StaticDecisions.ChapterSO_Current == null)
            {
                return;
            }

            Action_SetCollectableUISprite?.Invoke(StaticDecisions.ChapterSO_Current.Sprite_CollectableSpriteUI);
            Action_SetTimeText?.Invoke("0:00");
            Action_SetCollectableText?.Invoke(RuntimeUtility.int_CollectableAmount);
            GameObject_WinScreen.SetActive(false);
        }
        
        [ContextMenu("Toggle UI")]
        public void ToggleAllUI()
        {
            RuntimeUtility.Action_PauseGame?.Invoke(!GameObject_MainScreen.activeInHierarchy);
            GameObject_WinScreen.SetActive(!GameObject_MainScreen.activeInHierarchy);
            GameObject_MainScreen.SetActive(!GameObject_MainScreen.activeInHierarchy);
        }

#region GAMEPLAY UI
        protected void FinishGame()
        { 
            PauseScreen(false);
            GameObject_WinScreen.SetActive(true);
            TMP_Text_EndTime.text = $"<b>Level Complete</b>\n\nCollected all {StaticDecisions.ChapterSO_Current.string_CollectableName}s\nTime: {reign.Time.ConvertToMMSS((int)RuntimeUtility.float_GameTime)}";
            MasterSystem.Instance._CursorSystem.SetCursor("Arrow", CursorLockMode.None, true);  
        }
        protected void SetCollectableUISprite(Sprite SPRITE)
        {
            Image_Collectable.sprite = SPRITE;
        }
        protected void SetTimeText(string TEXT)
        {
            TMP_Text_Time.text = TEXT;
        }

        protected void SetCollectableText(int AMT)
        {
            TMP_Text_Collectable.text = $"{AMT}/{StaticDecisions.ChapterSO_Current.int_CollectableAmount} {StaticDecisions.ChapterSO_Current.string_CollectableName}s";
        } 
#endregion

#region PAUSED GAME
        void PauseScreen(bool GAMEPAUSED)
        {
            GameObject_PauseScreen.SetActive(GAMEPAUSED);

            if(!GAMEPAUSED)
            {
                MasterSystem.Instance._CursorSystem.SetCursor("Arrow", CursorLockMode.Locked, false);
            }
            else
            {
                MasterSystem.Instance._CursorSystem.SetCursor("Arrow", CursorLockMode.None, true);   
            }
        }

        public void ResumeGameFromUI()
        {
            RuntimeUtility.Action_PauseGame?.Invoke(false);
        }
        public void RestartFromUI()
        {
            LoadingSystem.Action_TryLoad?.Invoke(SceneManager.GetActiveScene().name, 1, 1);
        }
        public void ExitFromUI()
        {
            LoadingSystem.Action_TryLoad?.Invoke("Launch", 1, 1);
        }
#endregion
    }
}
