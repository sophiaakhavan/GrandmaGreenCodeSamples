using System;
using System.Collections;
using System.Collections.Generic;
using GrandmaGreen.Garden;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GrandmaGreen.Collections {
    
    // Used as a wrapper for all the inventory item types.
    public interface IInventoryItem
    {
        public ushort itemID { get; set; }
        public ItemType itemType { get; set; }
        
        public string itemName { get; set; }
        
        public int quantity { get; set; }

        public bool isBeingSold { get; set; }

        public string GetQuantityToString();

    }
    
    [Serializable]
    public enum ItemType
    {
        Plant,
        Seed,
        Tool,
        Decor
    }
    
    [Serializable]
    public struct Plant : IInventoryItem
    {
        // Item ID.
        public ushort itemID { get; set; }
        
        // This item type;
        public ItemType itemType { get; set; }

        // Name of object.
        public string itemName { get; set; }
        
        // Amount of the object present in the inventory.
        public int quantity { get; set; }

        public bool isBeingSold { get; set; }

        public Genotype plantGenotype;

        public Plant(ushort id, string name, Genotype genotype)
        {
            itemType = ItemType.Plant;
            
            itemID = id;
            itemName = name;
            quantity = 1;
            plantGenotype = genotype;

            isBeingSold = false;
        }
        
        public string GetQuantityToString()
        {
            return quantity.ToString();
        }

        public override bool Equals(object obj) =>
            obj is Plant other
            && other.itemID == itemID
            && other.itemType == itemType
            && other.plantGenotype.Equals(plantGenotype);
    }

    [Serializable]
    public struct Seed : IInventoryItem
    {
        [ShowInInspector]
        // Item ID.
        public ushort itemID { get; set; }
        
        // This item type;
        [ShowInInspector] public ItemType itemType { get; set; }

        // Name of object.
        [ShowInInspector]
        public string itemName { get; set; }
        
        // Amount of the object present in the inventory.
        public int quantity { get; set; }

        public bool isBeingSold { get; set; }

        public Genotype seedGenotype;

        public string trait;

        public Seed(ushort id, string name, Genotype genotype)
        {
            itemType = ItemType.Seed;
            
            itemID = id;
            itemName = name;
            quantity = 1;
            seedGenotype = genotype;
            trait = "";

            isBeingSold = false;
            // trait = GetTrait();
        }

        public string GetTrait()
        {
            Seed s = this;
            int i = CollectionsSO.LoadedInstance.PlantGenotypeMasterList.FindIndex( other => s.Equals(other));
            Seed o = CollectionsSO.LoadedInstance.PlantGenotypeMasterList[i];
            return o.trait;
        }
        
        public string GetQuantityToString()
        {
            return quantity.ToString();
        }
        
        public override bool Equals(object obj) =>
            obj is Seed other
            && other.itemID == itemID
            && other.itemType == itemType
            && other.seedGenotype.Equals(seedGenotype);
    }
    
    [Serializable]
    public struct Tool : IInventoryItem
    {
        // Item ID.
        public ushort itemID { get; set; }

        // This item type;
        public ItemType itemType { get; set; }

        // Name of object.
        public string itemName { get; set; }
        
        // Amount of the object present in the inventory.
        public int quantity { get; set; }

        public bool isBeingSold { get; set; }

        public Tool(ushort id, string name)
        {
            itemType = ItemType.Tool;

            itemID = id;
            itemName = name;
            quantity = 1;

            isBeingSold = false;
        }
        
        public Tool(ushort id, string name, int num)
        {
            itemType = ItemType.Tool;
            
            itemID = id;
            itemName = name;
            quantity = num;

            isBeingSold = false;
        }

        public string GetQuantityToString()
        {
            return quantity.ToString();
        }

        public override bool Equals(object obj) =>
            obj is IInventoryItem other && other != null && other.itemID == itemID && other.itemType == itemType;
    }
    
    [Serializable]
    public struct Decor : IInventoryItem
    {
        // Item ID.
        public ushort itemID { get; set; }
        
        // This item type;
        public ItemType itemType { get; set; }

        // Name of object.
        public string itemName { get; set; }
        
        // Amount of the object present in the inventory.
        public int quantity { get; set; }

        public bool isBeingSold { get; set; }

        public bool isFixture { get; set; }

        public Decor(ushort id, string name)
        {
            itemType = ItemType.Decor;

            itemID = id;
            itemName = name;
            quantity = 1;

            isBeingSold = false;
            isFixture = false;
        }
        
        public Decor(ushort id, string name, int num)
        {
            itemType = ItemType.Decor;
            
            itemID = id;
            itemName = name;
            quantity = num;

            isBeingSold = false;
            isFixture = false;
        }
        
        public string GetQuantityToString()
        {
            return quantity.ToString();
        }
        
        public override bool Equals(object obj) =>
            obj is IInventoryItem other && other != null && other.itemID == itemID && other.itemType == itemType;
    }
}