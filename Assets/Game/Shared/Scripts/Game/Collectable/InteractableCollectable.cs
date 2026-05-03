using reign;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace FBH
{
    public class InteractableCollectable : Interactable
    {
        SoundPlaybacker SoundPlaybacker_Player;
        [SerializeField] SpriteRenderer SpriteRenderer_Renderer;
        [SerializeField] string string_CollectionSound = "player_chew";
        [SerializeField] UnityEvent UnityEvent_OnCollected;
        [SerializeField] int int_AddAmount = 1;
        void Start()
        {
            SpriteRenderer_Renderer = GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer_Renderer.sprite = StaticDecisions.ChapterSO_Current.Sprite_CollectableSprite;
        }
        void Collect()
        {
            if(RuntimeUtility.int_CollectableAmount >= StaticDecisions.ChapterSO_Current.int_CollectableAmount)
            {
                return;
            }

            RuntimeUtility.Action_AddCollectable?.Invoke(int_AddAmount);
            PlayerController.Action_AddStamina?.Invoke(100);
            
            if(RuntimeUtility.int_CollectableAmount == StaticDecisions.ChapterSO_Current.int_CollectableAmount)
            {
                PlayerController.SoundPlaybacker_PlayerSound.PlayOneShot(StaticDecisions.ChapterSO_Current.string_CollectionTotalSound, 0.5f);
            } 
            else
            {
                PlayerController.SoundPlaybacker_PlayerSound.PlayOneShot(string_CollectionSound, 0.5f);
            }
            
            UnityEvent_OnCollected?.Invoke();
            Destroy(gameObject);
        }
        public override void Use()
        {
            base.Use();
            Collect();
        }
    }
}
