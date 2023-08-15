using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandmaGreen.Shopkeeping
{
    using Timer = TimeLayer.TimeLayer;
    [CreateAssetMenu(menuName = "GrandmaGreen/Shopkeeping/ShoppingManager")]
    public class ShoppingManager : ScriptableObject
    {
        public List<ShopItem> currGardenList;
        public List<ShopItem> currDecorList;
        
        public GardeningShopUIController gardenController;
        public DecorShopUIController decorController;
        public GardenUnlockData gardenUnlockData;

        [SerializeField]
        public Timer shopTimer;

        public void StartShop()
        {
            if (gardenController == null)
            {
                gardenController = new GardeningShopUIController();
                decorController = new DecorShopUIController(gardenUnlockData);
            }
            currGardenList = gardenController.itemList;
            currDecorList = decorController.itemList;

            shopTimer.Resume(true);
            shopTimer.onTick += gardenController.UpdateCycle;
            shopTimer.onTick += decorController.UpdateCycle;
        }

        public List<ShopItem> RegenerateGardenShop()
        {
            gardenController.GenerateGardenList();
            currGardenList = gardenController.itemList;
            return currGardenList;
        }

        public List<ShopItem> RegenerateDecorShop()
        {
            decorController.GenerateDecorList();
            currDecorList = decorController.itemList;
            return currDecorList;
        }

        public double GetTimeLeft()
        {
            return shopTimer.tickSeconds - shopTimer.GetTickValue();
        }
    }
}
