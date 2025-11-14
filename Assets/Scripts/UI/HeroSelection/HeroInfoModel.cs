using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace GameOff2025.Assets.Scripts.UI.HeroSelection
{
    public class HeroInfoModel
    {
        public string heroName { get; private set; }
        public Image heroIcon { get; private set; }
        public string heroDescription { get; private set; }
        public bool isRecruited { get; private set; }

        public HeroInfoModel(string name, Image icon, string description)
        {
            heroName = name;
            heroIcon = icon;
            heroDescription = description;
            isRecruited = false;
        }

        public void Recruit()
        {
            isRecruited = true;
        }

        public void Dismiss()
        {
            isRecruited = false;
        }
    }
}