using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace GrandmaGreen
{
    [System.Serializable]
    public struct TransitionData
    {
        public Collider cameraBounds;
        public CinemachineVirtualCamera camera;
        public Transform[] enterancePoints;
    }

    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/GardenTransitionData", fileName = "GardenTransitionData")]
    public class GardenTransitionData : ScriptableObject
    {
        public TransitionData[] transitions = new TransitionData[4];

        public void RegisterTransition(TransitionData transitionData, int index)
        {
            transitions[index] = transitionData;
        }
    }
}
