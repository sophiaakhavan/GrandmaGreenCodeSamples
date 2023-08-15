using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Input;
using DG.Tweening;
using GrandmaGreen.Collections;
using UnityEngine.EventSystems;
using GrandmaGreen.UI;
using GrandmaGreen.UI.Collections;
using UnityEngine.Tilemaps;

namespace GrandmaGreen.Garden
{
    //public enum FixtureType : ushort
    //{
    //    Mailbox = 1,
    //    Cottage,
    //    Fence
    //}
    /// <summary>
    /// Handles the logic of fixtures in customization
    /// </summary>
    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/FixtureCustomizer")]
    public class FixtureCustomizer : ScriptableObject
    {
        [Header("References")]
        [SerializeField] FixtureItem fixtureItemPrefab;
        //[SerializeField] FixtureItem fixtureMailboxItemPrefab;
        [SerializeField] Collections.DecorationId debugDecor;
        [SerializeField] TileStore tileStore;
        [SerializeField] PointerState pointerState;
        [SerializeField] GameObject grandmaRig;

        [Header("Settings")]
        [SerializeField] float colliderSizeModifier = 1.0f;
        [SerializeField] float validCheckTime = 0.05f;
        //[SerializeField] LayerMask decorLayerMask;
        [SerializeField] LayerMask fixtureLayerMask;

        [SerializeField] Material activeMaterial;
        [SerializeField] Material defaultMaterial;
        [SerializeField] Color validColor;
        [SerializeField] Color invalidColor;

        public TabbedInventory inventoryUI;

        public FixtureItem defaultGate; //these are also what we will use to determine positions of defaults
        public FixtureItem defaultCottage;
        public FixtureItem defaultMailbox;

        public FixtureItem GenerateFixtureItem() => GenerateFixtureItem(debugDecor);

        Plane xyPlane = new Plane(-Vector3.forward, Vector3.zero);

        public FixtureItem GenerateFixtureItem(Collections.DecorationId decorID)
        {
            FixtureType f = CollectionsSO.LoadedInstance.GetFixtureType(decorID);
            FixtureItem fixtureItem;
            fixtureItem = Instantiate(fixtureItemPrefab);
            fixtureItem.fixtureType = f;

            fixtureItem.decorID = decorID;
            Sprite decorSprite = CollectionsSO.LoadedInstance.GetSprite((ushort)decorID);
            fixtureItem.GetComponentInChildren<SpriteRenderer>().sprite = decorSprite;

            Vector3 colliderSize = fixtureItem.boundsCollider.size;
            colliderSize.x = decorSprite.bounds.size.x * colliderSizeModifier;
            fixtureItem.boundsCollider.size = colliderSize;

            //fixtureItem.transform.localScale = grandmaRig.transform.localScale;
            

            if (decorID == DecorationId.MailboxDefault || decorID == DecorationId.Default || decorID == DecorationId.GateofDefault)
            {
                fixtureItem.isDefault = true;
            }
            else
            {
                fixtureItem.isDefault = false;
            }

            return fixtureItem;
        }

        /// <summary>
        /// A fixture item that is being dragged in customization is a valid state if it is overlapping with
        /// the collider of the fixture bounds for which it belongs to. (ie cottage fixture --> cottage fixture slot collider).
        /// </summary>
        /// <param name="fixtureItem"></param>
        /// <param name="decorArea"></param>
        /// <returns></returns>
        public bool CheckValidState(BoxCollider fixtureItem, GardenAreaController decorArea, FixtureType currFixtureType)
        {

            //determine size of fixture item being dragged:
            //Vector3Int fixtureItemSize = Vector3Int.one;
            //fixtureItemSize.x = Mathf.CeilToInt(fixtureItem.bounds.size.x);
            //fixtureItemSize.y = Mathf.CeilToInt(fixtureItem.bounds.size.y);

            ////figure out if it is overlapping with fixture slot:

            ////check each collider in the overlapbox for fixtureItem.bounds and fixtureLayerMask
            //foreach (Collider coll in Physics.OverlapBox(fixtureItem.bounds.center, fixtureItem.bounds.extents, Quaternion.identity, fixtureLayerMask))
            //{
            //    if (coll != fixtureItem && coll.gameObject.GetComponent<FixtureItem>().fixtureType == currFixtureType)
            //    {
            //        return true;
            //    }
            //}

            //return false;

            foreach (Collider coll in Physics.OverlapBox(fixtureItem.bounds.center, fixtureItem.bounds.extents, Quaternion.identity, fixtureLayerMask))
            {
                if (coll == fixtureItem && coll.gameObject.GetComponent<FixtureItem>().isPlaced)
                {
                    return false;
                }
            }

            return true;
        }

        Coroutine customizationState;

        public void EnterFixtureCustomizationState(GardenAreaController decorArea, FixtureItem fixtureItem)
        {
            customizationState = decorArea.StartCoroutine(FixtureCustomizationHandler(decorArea, fixtureItem));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decorArea"></param>
        /// <param name="fixtureItem"></param>
        /// <returns></returns>
        IEnumerator FixtureCustomizationHandler(GardenAreaController decorArea, FixtureItem fixtureItem)
        {
            Vector3 cameraForwardAxis = Camera.main.transform.forward;
            WaitForSeconds waitForSeconds = new WaitForSeconds(validCheckTime);

            Ray ray = new Ray(pointerState.position, cameraForwardAxis);
            xyPlane.Raycast(ray, out float offset);
            Vector3 destination = pointerState.position + offset * cameraForwardAxis;
            destination.z = 0;

            fixtureItem.transform.position = destination;
            fixtureItem.sprite.material = activeMaterial;
            fixtureItem.DisableInteraction();


            bool isValid = false;
            do
            {
                ray = new Ray(pointerState.position, cameraForwardAxis);
                xyPlane.Raycast(ray, out offset);
                destination = pointerState.position + offset * cameraForwardAxis;
                destination.z = 0;

                fixtureItem.transform.position = destination;

                Physics.SyncTransforms();

                //boundsCollider is the box collider of the fixture item being dragged
                isValid = CheckValidState(fixtureItem.boundsCollider, decorArea, fixtureItem.fixtureType);

                fixtureItem.transform.position -= cameraForwardAxis * 8;

                fixtureItem.sprite.color = isValid ? validColor : invalidColor;

                yield return null;

            } while (fixtureItem && pointerState.phase != PointerState.Phase.NONE);

            fixtureItem.transform.position = destination;
            fixtureItem.sprite.material = defaultMaterial;
            fixtureItem.sprite.color = Color.white;
            fixtureItem.EnableInteraction();

            /* will tell event for customization attempt that it was successful or not, which will tell
             * GardenAreaController CompleteFixtureCustomization whether the customization was successful or not.
             * 
             */
            EventManager.instance.HandleEVENT_FIXTURE_CUSTOMIZATION_ATTEMPT(isValid);
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

        //    DeleteCurrentFixtureItem(); //will only actually delete if player drops the item into inventory
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
        //    KeepCurrentFixtureItem();
        //}
    }
}
