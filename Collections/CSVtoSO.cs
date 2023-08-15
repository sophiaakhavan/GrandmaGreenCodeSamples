using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using GrandmaGreen.SaveSystem;
using Sirenix.OdinInspector;
using System.Globalization;
using GrandmaGreen.Garden;

//#if (UNITY_EDITOR)


/// <summary>
/// This class populates and creates the Collections SO by reading the CSV file.
/// </summary>
namespace GrandmaGreen.Collections
{
    using Id = System.Enum;

    public class CSVtoSO
    {
        private static readonly string inventoryCSVPath = "/_GrandmaGreen/Scripts/Collections/CollectionsDatabase.csv";
        public const ushort SEED_ID_OFFSET = 1000;
        /// <summary>
        /// Function to populate and creates the Collections SO by reading the CSV file
        /// </summary>
        public static void GenerateCollectionsSO(CollectionsSO collections, TextAsset dataSheet)
        {
            string[] allLines = dataSheet.text.Split('\n');
            //File.ReadAllLines(Application.dataPath + inventoryCSVPath, System.Text.Encoding.Default);

            collections.ItemLookup = new Dictionary<ushort, ItemProperties>();
            collections.PlantLookup = new Dictionary<ushort, PlantProperties>();
            collections.PlantGenotypeMasterList = new List<Seed>();
            collections.DecorList = new List<Decor>();
            collections.DecorLookup = new Dictionary<ushort, Decor>();
            collections.FixtureList = new List<Decor>();
            collections.SpriteSheetCache = new Dictionary<string, Sprite[]>();
            collections.SingleSpriteCache = new Dictionary<string, Sprite>();
            //collections.CharacterLookup = new Dictionary<ushort, CharacterProperties>();
            //collections.SeedLookup = new Dictionary<ushort, SeedProperties>();
            

            //read the CSV
            for (int i = 2; i < allLines.Length; i++)
            {
                //current line in file
                var line = allLines[i].Split(',');

                if (line == null)
                    continue;
                if (!ushort.TryParse(line[0], out ushort csvID))
                    continue;

                string entryType = line[1];
                string name = line[2];
                string description = line[3];
                string tag = line[4];
                // int baseCost = Int32.Parse(line[5]);
                ushort.TryParse(line[5], out ushort baseCost); //this will be seed base cost for plants
                string seedDescription = line[6];
                string plantDecorType = line[7];
                string spritePath = "";
                string plantDesc1, plantDesc2, plantDesc3, plantDesc4, plantDesc5;
                plantDesc1 = line[11];
                plantDesc2 = line[12];
                plantDesc3 = line[13];
                plantDesc4 = line[14];
                plantDesc5 = line[15];
                ItemProperties itemProps = new ItemProperties();
                itemProps.name = name;
                itemProps.description = description;
                itemProps.itemType = entryType;
                itemProps.baseCost = baseCost;
                itemProps.spritePath = spritePath;
                itemProps.tag = tag;
                itemProps.decorType = plantDecorType;
                itemProps.isSellable = true;

                switch (entryType)
                {
                    case "Plant":
                        //Plant data
                        PlantProperties plantProps = new PlantProperties();
                        plantProps.plantDescriptions = new List<string>();
                        plantProps.name = name;
                        description = description.Replace(@"\", ",");
                        plantProps.description = description;
                        plantProps.spriteBasePath = "PLA_" + name.Replace(" ", "");
                        spritePath = "PLA_" + name.Replace(" ", "");
                        itemProps.spritePath = spritePath;
                        
                        //the base cost for plants is actually the base cost for seeds
                        itemProps.baseCost = baseCost;

                        //plant stats
                        plantProps.growthStages = 3;
                        ushort.TryParse(line[8], out ushort growthTime);
                        plantProps.growthTime = growthTime;
                        plantProps.waterPerStage = 200;

                        ushort.TryParse(line[9], out ushort maturePlantSellPrice);
                        plantProps.maturePlantSellPrice = maturePlantSellPrice;

                        List<string> genotypeList = new List<string>{ "AABB", "AABb", "AAbb", "AaBB", "AaBb", "Aabb", "aaBB", "aaBb", "aabb" };

                        
                        //flower
                        if (plantDecorType == "Flower")
                        {
                            plantProps.plantType = (PlantType)1;

                            //populate plant genotype master list -- plants of same type but diff genotype have the same id
                            for(int g=0; g<9; g++)
                            {
                                Seed seed = new Seed();
                                string genoString = genotypeList[g];
                                Genotype geno = new Genotype(genoString);
                                seed.seedGenotype = geno;
                                seed.itemID = csvID;
                                seed.itemType = ItemType.Seed;
                                string seedName = "";

                                //color
                                switch (genoString[2])
                                {
                                    case 'B':
                                        switch (genoString[3])
                                        {
                                            case 'B':
                                                switch(name)
                                                {
                                                    case "Rose":
                                                        seedName = "Red";
                                                        break;
                                                    case "Tulip":
                                                        seedName = "Red";
                                                        break;
                                                    case "Calla Lily":
                                                        seedName = "White";
                                                        break;
                                                    case "Dahlia":
                                                        seedName = "Red Yellow";
                                                        break;
                                                    case "Hyacinth":
                                                        seedName = "Pink";
                                                        break;
                                                    case "Pansy":
                                                        seedName = "Blue";
                                                        break;
                                                    case "Crocus":
                                                        seedName = "Blue Lavender";
                                                        break;
                                                }
                                                break;
                                            case 'b':
                                                switch (name)
                                                {
                                                    case "Rose":
                                                        seedName = "Pink";
                                                        break;
                                                    case "Tulip":
                                                        seedName = "Orange";
                                                        break;
                                                    case "Calla Lily":
                                                        seedName = "Yellow";
                                                        break;
                                                    case "Dahlia":
                                                        seedName = "Pink";
                                                        break;
                                                    case "Hyacinth":
                                                        seedName = "Blue";
                                                        break;
                                                    case "Pansy":
                                                        seedName = "Yellow Orange";
                                                        break;
                                                    case "Crocus":
                                                        seedName = "White";
                                                        break;
                                                }
                                                break;
                                        }
                                        break;
                                    case 'b':
                                        if (genoString[3] == 'b')
                                        {
                                            switch (name)
                                            {
                                                case "Rose":
                                                    seedName = "White";
                                                    break;
                                                case "Tulip":
                                                    seedName = "Yellow";
                                                    break;
                                                case "Calla Lily":
                                                    seedName = "Pink";
                                                    break;
                                                case "Dahlia":
                                                    seedName = "White";
                                                    break;
                                                case "Hyacinth":
                                                    seedName = "White";
                                                    break;
                                                case "Pansy":
                                                    seedName = "Violet";
                                                    break;
                                                case "Crocus":
                                                    seedName = "Yellow";
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                
                                seed.trait = seedName;
                                seedName += " " + name;
                                seed.itemName = seedName;
                                collections.PlantGenotypeMasterList.Add(seed);
                            }
                            plantProps.plantDescriptions.Add(plantDesc1);
                            plantProps.plantDescriptions.Add(plantDesc2);
                            plantProps.plantDescriptions.Add(plantDesc3);
                            plantProps.plantDescriptions.Add(plantDesc4);
                            plantProps.plantDescriptions.Add(plantDesc5);
                        }
                        //veggie
                        else if (plantDecorType == "Vegetable")
                        {
                            plantProps.plantType = (PlantType)2;

                            //populate plant genotype master list
                            for (int g = 0; g < 9; g++)
                            {
                                Seed seed = new Seed();
                                string genoString = genotypeList[g];
                                Genotype geno = new Genotype(genotypeList[g]);
                                seed.seedGenotype = geno;
                                seed.itemID = csvID;
                                seed.itemType = ItemType.Seed;
                                string seedName = "";

                                //shape
                                switch (genoString[2])
                                {
                                    case 'B':
                                        switch (genoString[3])
                                        {
                                            case 'B':
                                                seedName = "Long";
                                                break;
                                            case 'b':
                                                seedName = "Average";
                                                break;
                                        }
                                        break;
                                    case 'b':
                                        if (genoString[3] == 'b')
                                        {
                                            seedName = "Short";
                                        }
                                        break;

                                }
                                seed.trait = seedName;
                                seedName += " " + name;
                                seed.itemName = seedName;
                                collections.PlantGenotypeMasterList.Add(seed);
                            }
                            plantProps.plantDescriptions.Add(plantDesc1);
                            plantProps.plantDescriptions.Add(plantDesc2);
                            plantProps.plantDescriptions.Add(plantDesc3);
                        }
                        //fruit
                        else if (plantDecorType == "Fruit")
                        {
                            plantProps.plantType = (PlantType)3;

                            //populate plant genotype master list
                            for (int g = 0; g < 9; g++)
                            {
                                Seed seed = new Seed();
                                string genoString = genotypeList[g];
                                Genotype geno = new Genotype(genotypeList[g]);
                                seed.seedGenotype = geno;
                                seed.itemID = csvID;
                                seed.itemType = ItemType.Seed;
                                string seedName = "";

                                //variety
                                switch (genoString[2])
                                {
                                    case 'B':
                                        switch (genoString[3])
                                        {
                                            case 'B':
                                                switch (name)
                                                {
                                                    case "Apple":
                                                        seedName = "Red Delicious";
                                                        break;
                                                    case "Pear":
                                                        seedName = "Anjou";
                                                        break;
                                                    case "Watermelon":
                                                        seedName = "Crimson Sweet";
                                                        break;
                                                    case "Cherry":
                                                        seedName = "Bing";
                                                        break;
                                                    case "Plum":
                                                        seedName = "Danson";
                                                        break;
                                                    case "Peach":
                                                        seedName = "White";
                                                        break;
                                                    case "Blueberry":
                                                        seedName = "Legacy";
                                                        break;
                                                }
                                                break;
                                            case 'b':
                                                switch (name)
                                                {
                                                    case "Apple":
                                                        seedName = "Fuji";
                                                        break;
                                                    case "Pear":
                                                        seedName = "Asian";
                                                        break;
                                                    case "Watermelon":
                                                        seedName = "Moon and Stars";
                                                        break;
                                                    case "Cherry":
                                                        seedName = "Montmorency";
                                                        break;
                                                    case "Plum":
                                                        seedName = "Victoria";
                                                        break;
                                                    case "Peach":
                                                        seedName = "Nectarine";
                                                        break;
                                                    case "Blueberry":
                                                        seedName = "Pink Popcorn";
                                                        break;
                                                }
                                                break;
                                        }
                                        break;
                                    case 'b':
                                        if (genoString[3] == 'b')
                                        {
                                            switch (name)
                                            {
                                                case "Apple":
                                                    seedName = "Granny Smith";
                                                    break;
                                                case "Pear":
                                                    seedName = "Bartlett";
                                                    break;
                                                case "Watermelon":
                                                    seedName = "Tender Gold";
                                                    break;
                                                case "Cherry":
                                                    seedName = "Early Robin";
                                                    break;
                                                case "Plum":
                                                    seedName = "Mirabelle";
                                                    break;
                                                case "Peach":
                                                    seedName = "Donut";
                                                    break;
                                                case "Blueberry":
                                                    seedName = "Sunshine";
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                seed.trait = seedName;
                                seedName += " " + name;
                                seed.itemName = seedName;
                                collections.PlantGenotypeMasterList.Add(seed);
                            }
                            plantProps.plantDescriptions.Add(plantDesc1);
                            plantProps.plantDescriptions.Add(plantDesc2);
                            plantProps.plantDescriptions.Add(plantDesc3);
                        }

                        collections.PlantLookup.Add(csvID, plantProps);
                        collections.ItemLookup.Add(csvID, itemProps);

                        break;

                    case "Tool":
                        itemProps.spritePath = "Tools/GAR_" + name.Replace(" ", "_");
                        itemProps.baseCost = baseCost;
                        collections.ItemLookup.Add(csvID, itemProps);
                        break;

                    case "Decor":
                        ushort.TryParse(line[9], out ushort decorSellPrice);
                        itemProps.spritePath = "Decor/" + itemProps.decorType;
                        itemProps.baseCost = baseCost;
                        itemProps.sellCost = decorSellPrice;
                        if (baseCost == 0)
                        {
                            itemProps.isSellable = false; //for default decor items
                        }
                        collections.ItemLookup.Add(csvID, itemProps);
                        Decor decor = new Decor(csvID, name);
                        string isFixture = line[10];
                        if (isFixture.Substring(0,4) == "TRUE")
                        {
                            decor.isFixture = true;
                            collections.FixtureList.Add(decor);
                        }
                        collections.DecorList.Add(decor);
                        collections.DecorLookup.Add(csvID, decor);
                        break;

                    case "Character":
                        CharacterProperties characterProps = new CharacterProperties();
                        characterProps.name = name;
                        description = description.Replace(@"\", ",");
                        characterProps.description = description;
                        characterProps.spritePaths = new List<string>();
                        spritePath = "CHA_" + name.Replace(" ", "_");
                        characterProps.spritePaths.Add(spritePath);
                        itemProps.spritePath = spritePath;
                        //collections.CharacterLookup.Add(csvID, characterProps);
                        collections.ItemLookup.Add(csvID, itemProps);
                        break;

                    default:
                        Debug.Log("Entry Type not valid");
                        break;

                }
            }

        }

    }
}
//#endif