using System.Collections;
using System.Collections.Generic;
using reign;
using UnityEngine;

namespace FBH
{
    [RequireComponent(typeof(SoundPlaybacker))]
    public class SoundPlayer : MonoBehaviour
    {
        // To be used in UI elements.
        [SerializeField] SoundPlaybacker SoundPlaybacker;
        public void Play(string NAME)
        {
            SoundPlaybacker.PlayOneShot(NAME);
        }
    }
}
