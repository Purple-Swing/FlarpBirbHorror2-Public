using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using reign;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace FBH
{
    public class Options : MonoBehaviour, IDataHandler
    {
        [SerializeField] AudioMixer AudioMixer_Game;
        [SerializeField] Slider Slider_Volume, Slider_FOV;
        [SerializeField] TMP_Dropdown TMP_Dropdown_GraphicsQuality;
        [SerializeField] GameObject GameObject_Container;
        int int_CurrentIndex;
        [SerializeField] List<GameObject> List_Tabs;
        void Start()
        {
            GameObject_Container.SetActive(false);
        }
        public void OpenMenu()
        {
            GameObject_Container.SetActive(true);
            GoToTab(0);
        }
        public void SaveGame()
        {
            MasterSystem.Instance._SaveSystem.SaveGameData();
        }
        public void GoToTab(int INDEX)
        {
            int_CurrentIndex = INDEX;
            for (int i = 0; i < List_Tabs.Count; ++i)
            {
                List_Tabs[i].SetActive(i == INDEX);
            }
        }
        public void ClickityClackity(AudioSource SOURCE, Slider SLIDER)
        {
            SOURCE.pitch = Mathf.Lerp(0.8f, 1.2f, SLIDER.value / 100f);
        }
        public void VolumeSliderChange(AudioSource SOURCE)
        {
            if(!GameObject_Container.activeInHierarchy) return;
            SOURCE.PlayOneShot(MasterSystem.Instance._SoundSystem.GetSound("move"));
            AudioMixer_Game.SetFloat("Volume", Mathf.Log10(Slider_Volume.value / 100f) * 20);
            ClickityClackity(SOURCE, Slider_Volume);
        }
        public void FOVSliderChange(AudioSource SOURCE)
        {
            if(!GameObject_Container.activeInHierarchy) return;
            SOURCE.PlayOneShot(MasterSystem.Instance._SoundSystem.GetSound("move"));
            PlayerController.Action_SetFOV?.Invoke(Slider_FOV.value);
            ClickityClackity(SOURCE, Slider_FOV);
        }
        public void LoadData(GameData DATA)
        {
            TMP_Dropdown_GraphicsQuality.value = (int)DATA.enum_GraphicsQuality_Game;
            Slider_FOV.value = DATA.float_FOV;
            Slider_Volume.value = DATA.int_MasterVolume;
        }
        public void SaveData(ref GameData DATA)
        {
            DATA.enum_GraphicsQuality_Game = (GameData.enum_GraphicsQuality)TMP_Dropdown_GraphicsQuality.value;
            DATA.float_FOV = Slider_FOV.value;
            DATA.int_MasterVolume = (int)Slider_Volume.value;
        }
    }
}
