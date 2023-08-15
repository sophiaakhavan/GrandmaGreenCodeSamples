using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GrandmaGreen
{
    public class GardenUnlockController : MonoBehaviour
    {
        public GardenUnlockData unlockData;
        // 1 -- left, 2 -- top, 3 -- right

        public GameObject[] unlockObjects;

        public MeshRenderer[] gardenMeshes;

        void Start()
        {
            if (gardenMeshes.Length > 0)
            {
                unlockData.boundMin = gardenMeshes[0].bounds.min;
                unlockData.boundMax = gardenMeshes[0].bounds.max;
            }

            for (int i = 0; i < unlockObjects.Length; i++)
            {
                if(unlockData.CheckUnlockState( i+1))
                {
                    unlockObjects[i].SetActive(false);

                    unlockData.boundMin.x = Math.Min(unlockData.boundMin.x,gardenMeshes[i+1].bounds.min.x);
                    unlockData.boundMin.y = Math.Min(unlockData.boundMin.y,gardenMeshes[i+1].bounds.min.y);
                    unlockData.boundMax.x = Math.Max(unlockData.boundMax.x, gardenMeshes[i+1].bounds.max.x);
                    unlockData.boundMax.y = Math.Max(unlockData.boundMax.y, gardenMeshes[i+1].bounds.max.y);
                }
            }
            Debug.Log("Bound Min: " + unlockData.boundMin);
            Debug.Log("Bound Max: " + unlockData.boundMax);
        }

    }
}
