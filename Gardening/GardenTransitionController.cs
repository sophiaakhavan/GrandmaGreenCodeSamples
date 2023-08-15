using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandmaGreen
{
    public class GardenTransitionController : MonoBehaviour
    {
        public GardenTransitionData gardenTransitionData;
        public int areaIndex;
        public TransitionData transitionData;

        void Awake()
        {
            gardenTransitionData.RegisterTransition(transitionData, areaIndex);
        }
    }
}
