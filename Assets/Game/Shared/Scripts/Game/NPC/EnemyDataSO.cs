using UnityEngine;

namespace FBH
{
    [CreateAssetMenu(fileName = "FBH/Enemy Data", menuName = "FBH/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        public float float_Damage = 0f, float_ViewDistance = 20f, float_FOV = 80f, float_MovementSpeed = 4;
        public AudioClip AudioClip_AmbientSound;
        public bool bool_CanBreakDoors;
        public float float_SphereScale = 15f;
    }
}
