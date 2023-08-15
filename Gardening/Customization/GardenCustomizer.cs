using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Core.Input;
using DG.Tweening;
using SpookuleleAudio;
using GrandmaGreen.Collections;
using UnityEngine.EventSystems;
using GrandmaGreen.UI;
using GrandmaGreen.UI.Collections;

namespace GrandmaGreen.Garden
{
    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/GardenCustomizer")]
    public class GardenCustomizer : ScriptableObject
    {
        [Header("References")]
        [SerializeField] GardenDecorItem decorItemPrefab;
        [SerializeField] Collections.DecorationId debugDecor;
        [SerializeField] TileStore tileStore;
        [SerializeField] PointerState pointerState;
        [SerializeField] GameObject grandmaRig;
        [SerializeField] FixtureCustomizer fixtureCustomizer;

        [Header("Settings")]
        [SerializeField] float colliderSizeModifier = 1.05f;
        [SerializeField] float validCheckTime = 0.05f;
        [SerializeField] LayerMask decorLayerMask;
        [SerializeField] LayerMask fixtureLayerMask;

        [SerializeField] Material activeMaterial;
        [SerializeField] Material defaultMaterial;
        [SerializeField] Color validColor;
        [SerializeField] Color invalidColor;

        [SerializeField] GardenManager gardenManager;

        public TabbedInventory inventoryUI;

        public GardenDecorItem GenerateDecorItem() => GenerateDecorItem(debugDecor);

        Plane xyPlane = new Plane(-Vector3.forward, Vector3.zero);

        public GardenDecorItem GenerateDecorItem(Collections.DecorationId decorID)
        {
            GardenDecorItem decorItem = Instantiate(decorItemPrefab);

            decorItem.decorID = decorID;
            Sprite decorSprite = CollectionsSO.LoadedInstance.GetSprite((ushort)decorID);
            decorItem.GetComponentInChildren<SpriteRenderer>().sprite = decorSprite;

            Vector3 colliderSize = decorItem.boundsCollider.size;
            colliderSize.x = decorSprite.bounds.size.x * colliderSizeModifier;
            decorItem.boundsCollider.size = colliderSize;

            Vector3 decorScale = grandmaRig.transform.localScale;
            decorScale.z = decorScale.x;
            decorItem.transform.localScale = decorScale;

            decorItem.interactable.size = decorSprite.bounds.size;
            decorItem.interactable.center = new Vector3(0, decorItem.interactable.size.y / 2, 0);
            decorItem.interactable.transform.localEulerAngles = new Vector3(-45, 0, 0);

            return decorItem;
        }

        public bool CheckValidState(BoxCollider decorItem, GardenAreaController decorArea)
        {
            Vector3Int tileBlockSize = Vector3Int.one;
            tileBlockSize.x = Mathf.CeilToInt(decorItem.bounds.size.x / decorArea.tilemap.cellSize.x);/// decorArea.tilemap.cellSize.x);
            tileBlockSize.y = Mathf.CeilToInt(decorItem.bounds.size.y / decorArea.tilemap.cellSize.y); /// decorArea.tilemap.cellSize.y);

            Vector3Int tileBlockOrigin = Vector3Int.zero;
            tileBlockOrigin = decorArea.tilemap.WorldToCell(decorItem.transform.position);
            //tileBlockOrigin.x -= tileBlockSize.x / 2;

            BoundsInt colliderBounds = new BoundsInt(tileBlockOrigin, tileBlockSize);


            TileBase[] m_tileBlock = decorArea.tilemap.GetTilesBlock(colliderBounds);

            foreach (TileBase tileBase in m_tileBlock)
            {
                TileBase origin = decorArea.tilemap.GetTile(tileBlockOrigin);
                if (!tileStore[origin].pathable) return false;
                break;
                if (!tileStore[tileBase].pathable)// || tileStore[tileBase].occupied || tileStore[tileBase].plantable)
                    return false;
            }

            //if this decor item collides with anything in fixture layer mask, return false
            foreach (Collider coll in Physics.OverlapBox(decorItem.bounds.center, decorItem.bounds.extents, Quaternion.identity, fixtureLayerMask))
            {
                if (coll != decorItem)
                {
                    return false;
                }
            }


            return true;

            foreach (Collider coll in Physics.OverlapBox(decorItem.bounds.center, decorItem.bounds.extents, Quaternion.identity, decorLayerMask))
            {
                if (coll != decorItem)
                    return false;
            }


            return true;
        }

        Coroutine customizationState;

        public void EnterDecorCustomizationState(GardenAreaController decorArea, GardenDecorItem decorItem)
        {
            customizationState = decorArea.StartCoroutine(DecorCustomizationHandler(decorArea, decorItem));
        }

        /// <summary>
        /// TODO: Dont calculate per frame
        /// TODO: Color sprite
        /// </summary>
        /// <param name="decorArea"></param>
        /// <param name="decorItem"></param>
        /// <returns></returns>
        IEnumerator DecorCustomizationHandler(GardenAreaController decorArea, GardenDecorItem decorItem)
        {

            Vector3 cameraForwardAxis = Camera.main.transform.forward;
            WaitForSeconds waitForSeconds = new WaitForSeconds(validCheckTime);

            Ray ray = new Ray(pointerState.position, cameraForwardAxis);
            xyPlane.Raycast(ray, out float offset);
            Vector3 destination = pointerState.position + offset * cameraForwardAxis;
            destination.z = 0;

            decorItem.transform.position = destination;
            decorItem.sprite.material = activeMaterial;
            decorItem.DisableInteraction();

            bool isValid = false;
            do
            {
                ray = new Ray(pointerState.position, cameraForwardAxis);
                xyPlane.Raycast(ray, out offset);
                destination = pointerState.position + offset * cameraForwardAxis;
                destination.z = 0;

                decorItem.transform.position = destination;

                Physics.SyncTransforms();

                isValid = CheckValidState(decorItem.boundsCollider, decorArea);

                decorItem.transform.position -= cameraForwardAxis * 8;

                decorItem.sprite.color = isValid ? validColor : invalidColor;

                yield return null;

            } while (decorItem && pointerState.phase != PointerState.Phase.NONE);

            decorItem.transform.position = destination;
            decorItem.sprite.material = defaultMaterial;
            decorItem.sprite.color = Color.white;
            decorItem.EnableInteraction();

            EventManager.instance.HandleEVENT_CUSTOMIZATION_ATTEMPT(isValid);
        }

        //public void SlideOpenInventory()
        //{
        //    Debug.Log("opening inventory slide");
        //    if (inventoryUI == null)
        //    {
        //        var inv = GameObject.Find("/GAME SYSTEMS/UI/Inventory UI");
        //        inventoryUI = inv.GetComponent<TabbedInventory>();
        //        Debug.Log("getting the inventory UI");
        //    }
        //    inventoryUI.OpenUI();
            
        //    DeleteCurrentDecorItem();
        //}
        //public void SlideClosedInventory()
        //{
        //    Debug.Log("closing inventory slide");
        //    //need to first check if the player is holding an item or not
        //    //if holding an item while this function is called, only close the inventory
        //    //otherwise, put item into inventory and THEN close inventory
        //    if (inventoryUI == null)
        //    {
        //        var inv = GameObject.Find("/GAME SYSTEMS/UI/Inventory UI");
        //        inventoryUI = inv.GetComponent<TabbedInventory>();
        //        Debug.Log("getting the inventory UI");
        //    }
        //    inventoryUI.CloseUI();
        //    KeepCurrentDecorItem();
        //}
    }
}
