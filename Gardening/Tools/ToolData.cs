using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpookuleleAudio;

namespace GrandmaGreen.Garden
{
    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/Tool Data")]
    public class ToolData : ScriptableObject
    {
        public int toolIndex;
        public string toolName;
        public Sprite icon;
        public ASoundContainer selectedSFX;
        public ASoundContainer[] toolSFX;
    }
}
