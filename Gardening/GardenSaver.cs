using GrandmaGreen.SaveSystem;
using System.Collections.Generic;
using UnityEngine;
using GrandmaGreen.Collections;

namespace GrandmaGreen.Garden
{
    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/GardenSaver")]
    public class GardenSaver : ObjectSaver
    {
        private readonly int plantKey = 0;
        private readonly int plantValues = 1;
        private readonly int tiles = 2;
        private readonly int decor = 3;

        Dictionary<Vector3Int, int> tileStateLookup;

        public void Initialize()
        {
            tileStateLookup = new Dictionary<Vector3Int, int>();

            if (componentStores.Count == 0)
            {
                CreateNewStore(typeof(Vector3Int));
                CreateNewStore(typeof(PlantState));
                CreateNewStore(typeof(TileState));
                CreateNewStore(typeof(DecorState));
                return;
            }

            for (int i = 0; i < ((ComponentStore<TileState>)componentStores[tiles]).components.Count; i++)
            {
                tileStateLookup.Add(((ComponentStore<TileState>)componentStores[tiles]).components[i].cell, i);
            }

        }

        public bool ContainsKey(Vector3Int k)
        {
            return ((ComponentStore<Vector3Int>)componentStores[plantKey]).components.Contains(k);
        }

        public bool Remove(Vector3Int k)
        {
            if (ContainsKey(k))
            {
                int i = ((ComponentStore<Vector3Int>)componentStores[plantKey]).components.IndexOf(k);
                ((ComponentStore<Vector3Int>)componentStores[plantKey]).components.RemoveAt(i);
                ((ComponentStore<PlantState>)componentStores[plantValues]).components.RemoveAt(i);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            ((ComponentStore<Vector3Int>)componentStores[plantKey]).components.Clear();
            ((ComponentStore<PlantState>)componentStores[plantValues]).components.Clear();
        }

        public List<PlantState> Values()
        {
            return ((ComponentStore<PlantState>)componentStores[plantValues]).components;
        }

        public PlantState this[Vector3Int k]
        {
            get
            {
                return ((ComponentStore<PlantState>)componentStores[plantValues])
                    .components[((ComponentStore<Vector3Int>)componentStores[plantKey])
                    .components.IndexOf(k)];
            }
            set
            {
                if (ContainsKey(k))
                {
                    ((ComponentStore<PlantState>)componentStores[plantValues])
                        .components[((ComponentStore<Vector3Int>)componentStores[plantKey])
                        .components.IndexOf(k)] = value;
                }
                else
                {
                    ((ComponentStore<Vector3Int>)componentStores[plantKey])
                        .components.Add(k);
                    ((ComponentStore<PlantState>)componentStores[plantValues])
                        .components.Add(value);
                }
            }
        }

        public void SetTileState(TileState tileState)
        {
            if (componentStores.Count <= tiles)
                CreateNewStore(typeof(TileState));

            if (tileStateLookup.ContainsKey(tileState.cell))
                ((ComponentStore<TileState>)componentStores[tiles]).components[tileStateLookup[tileState.cell]] = tileState;
            else
            {
                tileStateLookup.Add(tileState.cell, ((ComponentStore<TileState>)componentStores[tiles]).components.Count);
                ((ComponentStore<TileState>)componentStores[tiles]).components.Add(tileState);
            }
        }

        public List<TileState> Tiles()
        {
            return ((ComponentStore<TileState>)componentStores[tiles]).components;
        }

        public void AddDecorState(DecorState decorState)
        {
            ComponentStore<DecorState> componentStore = (ComponentStore<DecorState>)componentStores[decor];

            componentStore.UpdateValue(-1, decorState);

            // ((ComponentStore<DecorState>)componentStores[decor]).components.Add(decorState);
        }

        public void RemoveDecorState(DecorState decorState)
        {
            ((ComponentStore<DecorState>)componentStores[decor]).components.Remove(decorState);
        }

        public List<DecorState> Decor()
        {
            return ((ComponentStore<DecorState>)componentStores[decor]).components;
        }

    }
}

