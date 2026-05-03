using System.Collections;
using System.Collections.Generic;
using reign;
using UnityEngine;
using UnityEngine.AI;

namespace FBH
{
    [RequireComponent(typeof(SoundPlaybacker), typeof(NavMeshObstacle))]
    public class InteractableDoor : Interactable
    {
        [SerializeField] float float_OpenTime = 3f;
        [SerializeField] Sprite[] Sprites_DoorSprites;
        [SerializeField] SpriteRenderer SpriteRenderer_Door;
        [SerializeField] BoxCollider BoxCollider_Door;
        SoundPlaybacker SoundPlaybacker_Door;
        [SerializeField] bool bool_Open;
        NavMeshObstacle NavMeshObstacle_Locked;
        public bool bool_Locked;

        void Awake()
        {
            NavMeshObstacle_Locked = GetComponent<NavMeshObstacle>();
            SpriteRenderer_Door = GetComponentInChildren<SpriteRenderer>();
            SoundPlaybacker_Door = GetComponent<SoundPlaybacker>();
        }
        void Start()
        {
            SpriteRenderer_Door.sprite = Sprites_DoorSprites[0];
            BoxCollider_Door.isTrigger = bool_Open;
            
            NavMeshObstacle_Locked.enabled = bool_Locked;
        }
        public override void Hover()
        {
            string_CursorNameCannotBeUsed = bool_Locked ? "Locked" : "Default";

            bool_CanBeUsed = !bool_Open && !bool_Locked;
        }
        public override void Use()
        {
            NavMeshObstacle_Locked.enabled = bool_Locked;

            if(bool_Locked)
            {
                SoundPlaybacker_Door.PlayOneShot("jammed_door", 0.5f);
                return;
            }

            if(bool_Open) 
            {
                return;
            }

            StartCoroutine(TryOpen());
        }
        public void BreakOpen()
        {
            SoundPlaybacker_Door.PlayOneShot("break_wood", 1.2f);
            bool_Locked = false;
            Use();
        }
        public void LockDoor(bool LOCKED)
        {
            bool_Locked = LOCKED;
            
            if(LOCKED)
            {
                SoundPlaybacker_Door.PlayOneShot("lock_door", 0.5f);
            }
            else
            {
                SoundPlaybacker_Door.PlayOneShot("unlock_door", 0.5f);
            }
        }
        IEnumerator TryOpen()
        {
            BoxCollider_Door.isTrigger = true;
            bool_Open = true;
            SpriteRenderer_Door.sprite = Sprites_DoorSprites[1];
            SoundPlaybacker_Door.PlayOneShot("open_door", 0.5f);
            
            yield return StartCoroutine(RuntimeUtility.WaitForGameSeconds(float_OpenTime));
            TryClose();
        }
        void TryClose()
        {
            BoxCollider_Door.isTrigger = false;
            bool_Open = false;
            SpriteRenderer_Door.sprite = Sprites_DoorSprites[0];
            SoundPlaybacker_Door.PlayOneShot("close_door", 0.5f);
        }
    }
}
