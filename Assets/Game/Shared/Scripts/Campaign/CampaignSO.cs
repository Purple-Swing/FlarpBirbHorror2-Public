using System.Collections.Generic;
using UnityEngine;

namespace FBH
{
    [CreateAssetMenu(fileName = "FBH/Campaign", menuName = "FBH/Campaign")]
    public class CampaignSO : ScriptableObject
    {
        public enum Enum_Campaigns
        {
            FBH,
            FBH2
        };
        public Enum_Campaigns Enum_Campaign = Enum_Campaigns.FBH2;
        public string string_LongName = "Flarp Birb Horror 2";
        public List<ChapterSO> ChapterSO_Chapters = new();
    }
}
