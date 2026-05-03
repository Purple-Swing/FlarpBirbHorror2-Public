using System.Collections;
using System.Collections.Generic;
using reign;
using UnityEngine;
using UnityEngine.Events;

namespace FBH
{
    [RequireComponent(typeof(SoundPlaybacker))]
    public class InteractableSwitch : Interactable
    {
        protected bool bool_On;
        protected SoundPlaybacker SoundPlaybacker_Sound;
        
        [Header("Settings")]
        [SerializeField] private Sprite[] Sprites_SwitchSprites;
        [SerializeField] private SpriteRenderer SpriteRenderer_Switch;
        public float float_OnTime = 4;
        public UnityEvent UnityEvent_OnSwitchedOn;
        public UnityEvent UnityEvent_OnSwitchedOff;

        void Start()
        {
            string_CursorNameCannotBeUsed = "Default";
            SoundPlaybacker_Sound = GetComponent<SoundPlaybacker>();
            SpriteRenderer_Switch.sprite = Sprites_SwitchSprites[0];
        }
        void TurnOff()
        {
            SwitchSprite(0);
            SoundPlaybacker_Sound.Play("switch_toggle", 0.6f, false, 1f);
            bool_On = false;
            UnityEvent_OnSwitchedOff?.Invoke();
        }
        void TurnOn()
        {
            SwitchSprite(1);
            SoundPlaybacker_Sound.Play("switch_toggle", 0.6f, false, 1f);
            bool_On = true;
            UnityEvent_OnSwitchedOn?.Invoke();

            if(float_OnTime > 0)
            {
                StartCoroutine(AutoTurnOff());
            }
        }

        IEnumerator AutoTurnOff()
        {
            yield return StartCoroutine(RuntimeUtility.WaitForGameSeconds(float_OnTime));
            TurnOff();
        }
        protected void SwitchSprite(int INDEX)
        {
            if (Sprites_SwitchSprites.Length >= 2)
            {
                SpriteRenderer_Switch.sprite = Sprites_SwitchSprites[INDEX];   
            }
        }

        public override void Use()
        {
            base.Use();
            
            if(bool_On)
            {
                return;
            }

            TurnOn();
        }
        public override void Hover()
        {
            bool_CanBeUsed = !bool_On;
        }
    }
}
