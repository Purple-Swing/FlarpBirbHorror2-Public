using System.Collections.Generic;
using reign;
using UnityEngine;
using UnityEngine.UI;

namespace FBH
{
    [RequireComponent(typeof(AssetLibrary))]
    public class PlayerCursor : MonoBehaviour
    {
        public float float_InteractionDistance {get; private set;} = 3f;
        [SerializeField] Camera Camera_PlayerCam;
        [SerializeField] AssetLibrary AssetLibrary_Cursors;
        [SerializeField] Image Image_Cursor;

        public void SetInteractionDistance(float NEW)
        {
            float_InteractionDistance = NEW;
        }
        public void SetCursor(string NAME, bool VISIBLE)
        { 
            Image_Cursor.enabled = VISIBLE;
            if (!VISIBLE) return;

            Image_Cursor.sprite = AssetLibrary_Cursors.GetAsset<Sprite>(NAME);
        }

        void OnEnable()
        {
            OriginSystem.Action_OnUpdate += Ray;
        }

        void OnDisable()
        {
            OriginSystem.Action_OnUpdate -= Ray;    
        }

        void Ray()
        {
            if(RuntimeUtility.bool_GamePaused) return;

            if(!Physics.Raycast(Camera_PlayerCam.transform.position, Camera_PlayerCam.transform.forward, out RaycastHit HIT, float_InteractionDistance))
            {
                SetCursor("Default", true);
                return;
            }
            else
            {
                if (!HIT.collider.gameObject.TryGetComponent(out Interactable COMPONENT))
                {
                    SetCursor("Default", true);
                    return;
                }

                SetCursor(COMPONENT.bool_CanBeUsed ? "Highlighted" : COMPONENT.string_CursorNameCannotBeUsed, true);

                if((COMPONENT.bool_CanBeUsed || COMPONENT.bool_CanAlwaysBeUsed) && InputSystem.GetInput("LeftMouse", InputSystem.enum_KeyType.Down))
                {
                    COMPONENT.Use();
                }

                COMPONENT.Hover();
            }
        }
    }
}
