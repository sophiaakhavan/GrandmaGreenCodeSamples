using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GrandmaGreen.Collections;
using UnityEngine.U2D.Animation;

namespace GrandmaGreen.Garden
{
    public class ToolEquip : MonoBehaviour
    {
        public PlayerToolData playerTools;
        public SpriteResolver toolSpriteResolver;
        public SpriteRenderer spriteRenderer;
        static readonly string SPRITE_CATEGORY = "Tools";

        void OnEnable()
        {
            playerTools.onToolSelected += SetEquippedTool;
            playerTools.onSeedEquipped += SetEquippedSeed;
            playerTools.onSeedEmpty += ClearSeed;
        }

        void OnDisable()
        {
            playerTools.onToolSelected -= SetEquippedTool;
            playerTools.onSeedEquipped -= SetEquippedSeed;
            playerTools.onSeedEmpty -= ClearSeed;
        }

        void SetEquippedTool(ToolData toolData)
        {
            if(toolData.toolIndex == 2 && EventManager.instance.HandleEVENT_INVENTORY_GET_FERTILIZER_COUNT() == 0)
            {
                toolSpriteResolver.SetCategoryAndLabel(SPRITE_CATEGORY, "Empty");
                return;
            }
            if (toolData.toolIndex == 3)
            {
                toolSpriteResolver.SetCategoryAndLabel(SPRITE_CATEGORY, "Empty");

                if (playerTools.equippedSeed != 0)SetEquippedSeed();
                return;
            }

            ClearSeed();
            toolSpriteResolver.SetCategoryAndLabel(SPRITE_CATEGORY, toolData.toolName);
        }

        void SetEquippedSeed()
        {
            spriteRenderer.sprite = CollectionsSO.LoadedInstance.GetSprite(playerTools.equippedSeed, playerTools.equippedSeedGenotype);
        }

        void ClearSeed()
        {
            spriteRenderer.sprite = null;
        }
    }
}
