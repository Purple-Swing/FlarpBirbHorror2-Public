using System;
using reign;
using UnityEditor;
using UnityEngine;

namespace FBH
{
    [CreateAssetMenu(fileName = "FBH/Campaign/Chapter", menuName = "FBH/Campaign/Chapter")]
    [Serializable]
    public class ChapterSO : ScriptableObject
    {
        public string string_ChapterTitle;        
        public string string_Scene;
        public CampaignSO CampaignSO_Related;
        public int int_ChapterNumber = 1;
        public string string_CollectableName;
        public string string_CollectionTotalSound;
        public int int_CollectableAmount;
        public Sprite Sprite_CollectableSprite;
        public Sprite Sprite_CollectableSpriteUI;
        public Sprite Sprite_ChapterPreview;
    }
}
