using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GrandmaGreen.SaveSystem;
using Newtonsoft.Json;

namespace GrandmaGreen
{
    public struct GardenUnlockState
    {
        public int gardenIndex;

        public override bool Equals(object obj) =>
            obj is GardenUnlockState other
            && gardenIndex == other.gardenIndex;
    }

    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/Unlock Data")]
        public class GardenUnlockData : ObjectSaver
    {
        [JsonIgnore] 
        public Vector3 boundMin;
        [JsonIgnore] 
        public Vector3 boundMax;
        
        /// <summary>
        /// Unlock gardens.
        /// </summary>
        public void UnlockGarden(int index)
        {
            GardenUnlockState unlockState = new GardenUnlockState()
            {
                gardenIndex = index
            };

            IComponentStore store = GetComponentStore<GardenUnlockState>();
            if (store == null)
            {
                CreateNewStore<GardenUnlockState>();
            }

            AddComponent<GardenUnlockState>(-1, unlockState);
        }
        
        /// <summary>
        /// Used to check the unlock state of a given garden. Does NOT tell you how many gardens are unlocked.
        /// </summary>
        /// <returns></returns>
        public bool CheckUnlockState(int index)
        {
            GardenUnlockState unlockState = new GardenUnlockState()
            {
                gardenIndex = index
            };
            return RequestData(-1, ref unlockState);
        }
        
        /// <summary>
        /// Returns the amount of gardens that are currently unlocked.
        /// </summary>
        /// <returns></returns>

        public int GetUnlockStateCount()
        {
            IComponentStore store = GetComponentStore<GardenUnlockState>();
            if (store == null)
            {
                return 0;
            }

            return ((ComponentStore<GardenUnlockState>)store).components.Count;
        }

        [ContextMenu("Debug Unlock")]
        void UnlockGardenTest() => UnlockGarden(1);
    }
}
