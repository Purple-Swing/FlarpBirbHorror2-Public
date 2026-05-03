using UnityEngine;

namespace FBH
{
    public class Interactable : MonoBehaviour
    {
        public bool bool_CanAlwaysBeUsed = false;
        public bool bool_CanBeUsed = true;
        public string string_CursorNameCannotBeUsed = "Locked";
        public virtual void Hover()
        {
            
        }
        public virtual void Use()
        {
        }
    }
}
