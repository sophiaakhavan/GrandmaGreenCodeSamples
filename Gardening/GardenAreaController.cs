using Core.Input;
using GrandmaGreen.Collections;
using GrandmaGreen.Entities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using SpookuleleAudio;
using System;
using static UnityEditor.Progress;
using Sirenix.Utilities;

namespace GrandmaGreen.Garden
{
    /// <summary>
    /// Responsible for functionality of a single Garden screen
    /// </summary>
    public class GardenAreaController : AreaController
    {

        [Header("Player References")]
        public PlayerToolData playerTools;

        [Header("Garden Management")]
        public GardenManager gardenManager;
        public GolemManager golemManager;
        public GardenCustomizer gardenCustomizer;
        public FixtureCustomizer fixtureCustomizer;

        public GardenVFX gardenVFX;
        public Cinemachine.CinemachineVirtualCamera customizationCamera;
        public GameObject BaseGardenCameraTarget;
        public GameObject WestGardenCameraTarget;
        public GameObject EastGardenCameraTarget;
        public GameObject NorthGardenCameraTarget;
        public GameObject customizationCanvas;
        public GameObject defaultPlantPrefab;
        public Dictionary<Vector3Int, GameObject> plantPrefabLookup;
        public float plantRowHorizontalOffset = 0;
        public List<PlantState> plantListDebug;
        public List<PlantState> wiltedPlantList;
        public GameObject defaultGate; //these are also what we will use to determine positions of defaults
        public GameObject defaultCottage;
        public GameObject defaultMailbox;
        public ushort currGateID; //possible id's are 4110-4118 -- initally set to defaults
        public ushort currCottageID; //4119-4129
        public ushort currMailboxID; //4102-4109
        public FixtureItem currentMailbox; //initially set to defaults
        public FixtureItem currentCottage;
        public FixtureItem currentFence;
        public Vector3 gatePos;
        public Vector3 cottagePos;
        public Vector3 mailboxPos;
        public Sprite defaultFenceSprite; //Fence01
        public GameObject leftFence; //set the sprite of the gameobject to the current fence sprite
        public GameObject rightFence;
        Vector3 shiftVec; // for fence
        Vector3 mailboxShiftVec; //for fence -- how much to move the mailbox over for current fence type
        Vector3 shiftGate; //for fence -- how much to shift the gate itself for the specific current fence gate
        Vector3 sizeFence; //how much to change sizing of fences
        Vector3 mailboxStretchVec; // how much to stretch the mailbox by
        Vector3 cottageShiftVec; // how much to move over the cottage for the current cottage type
        Vector3 cottageStretchVec; //how much to stretch/shrink the cottage for the current cottage type
        public Vector3 default_mailbox_pos; //hardcoding to keep track of original default position of mailbox
        public bool fenceShifted;
        public bool cottageShifted;
        public GameObject mailboxLogic;
        public GameObject mailboxPlaceholder;
        public GameObject gatePlaceholder;
        public GameObject cottagePlaceholder;
        public ASoundContainer successfulPlacementSFX;
        public ASoundContainer unsuccessfulPlacementSFX;
        public ASoundContainer flip1DecorSFX;
        public ASoundContainer flip2DecorSFX;
        public ASoundContainer openCustomizationSFX;
        public ASoundContainer closeCustomizationSFX;
        public ASoundContainer putInInventorySFX;
        public GameObject mailboxLogicObject;

        bool returnFromPause = true;

        [Header("Temporary Effects")]
        public bool m_inCustomizationMode = false;
        public bool m_inCultivisionMode = false;
        public float DropRate = 100;
        public float FertilizationBonus = 10;

        public event System.Action onGardenTick;
        public event System.Action<Vector3Int> onPlantWilted;
        public event System.Action<Vector3Int> onPlantDead;


        public override void Awake()
        {
            base.Awake();
            CollectionsSO.LoadedInstance.DEBUGLoadPlantProperties();
            //gardenManager.RegisterGarden(areaIndex);
            gardenManager.RegisterGarden(0);
            gardenManager.RegisterGarden(1);
            gardenManager.RegisterGarden(2);
            gardenManager.RegisterGarden(3);

            gardenManager.timers[areaIndex].Pause();

            playerTools.equippedSeed = 0;
            playerTools.ToolSelection(0);

            gardenVFX.Initalize();
        }

        void OnEnable()
        {
            gardenManager.timers[areaIndex].Resume(true);
            gardenManager.timers[areaIndex].onTick += IncrementTimer;

            onTilemapSelection += GardenTileSelected;
            EventManager.instance.EVENT_PLANT_UPDATE += PlantUpdate;
            EventManager.instance.EVENT_WATER_PLANT += WaterPlant;

            EventManager.instance.EVENT_TOGGLE_CULTIVISION_MODE += ToggleCultivisionMode;

            EventManager.instance.EVENT_INVENTORY_CUSTOMIZATION_START += StartDecorCustomization;
            EventManager.instance.EVENT_CUSTOMIZATION_ATTEMPT += CompleteDecorCustomization;

            EventManager.instance.EVENT_FIXTURE_CUSTOMIZATION_ATTEMPT += CompleteFixtureCustomization;

            EventManager.instance.EVENT_TOGGLE_CUSTOMIZATION_MODE += ToggleCustomizationMode;

            EventManager.instance.EVENT_CHANGE_GARDEN_INDEX += ChangeCustomizationCameraTarget;

            fixtureCustomizer.defaultCottage = defaultCottage.GetComponent<FixtureItem>();
            fixtureCustomizer.defaultGate = defaultGate.GetComponent<FixtureItem>();
            fixtureCustomizer.defaultMailbox = defaultMailbox.GetComponent<FixtureItem>();

            plantPrefabLookup = new Dictionary<Vector3Int, GameObject>();

            List<DecorState> savedDecorItems = gardenManager.GetDecor(areaIndex);
            bool defaultCottageIsSaved = false, defaultGateIsSaved = false, defaultMailboxIsSaved = false;
            foreach (DecorState decorState in savedDecorItems)
            {
                Decor decor = CollectionsSO.LoadedInstance.GetDecor((ushort)decorState.ID);
                if (decor.isFixture)
                {
                    if (decorState.ID == DecorationId.Default)
                    {
                        defaultCottageIsSaved = true;
                    }
                    else if (decorState.ID == DecorationId.MailboxDefault)
                    {
                        defaultMailboxIsSaved = true;
                    }
                    else if (decorState.ID == DecorationId.GateofDefault)
                    {
                        defaultGateIsSaved = true;
                    }
                }

            }

            if ((currCottageID == 0 && savedDecorItems.Count == 0) ||
                defaultCottageIsSaved) //0 is the assigned value when first instantiated. change to default fixture id in this case
            {
                if (currCottageID == 0 && savedDecorItems.Count == 0)
                {
                    FixtureItem fixItem = defaultCottage.GetComponent<FixtureItem>();
                    UpdateFixtureItem(fixItem);
                }
                currCottageID = (ushort)DecorationId.Default;
                defaultCottage.SetActive(true);
            }
            else
            {
                defaultCottage.SetActive(false);
            }

            if ((currGateID == 0 && savedDecorItems.Count == 0) ||
                defaultGateIsSaved)
            {
                if (currGateID == 0 && savedDecorItems.Count == 0)
                {
                    FixtureItem fixItem = defaultGate.GetComponent<FixtureItem>();
                    UpdateFixtureItem(fixItem);
                }
                currGateID = (ushort)DecorationId.GateofDefault;
                defaultGate.SetActive(true);
            }
            else
            {
                defaultGate.SetActive(false);
            }

            if ((currMailboxID == 0 && savedDecorItems.Count == 0) ||
                defaultMailboxIsSaved)
            {
                if (currMailboxID == 0 && savedDecorItems.Count == 0)
                {
                    FixtureItem fixItem = defaultMailbox.GetComponent<FixtureItem>();
                    UpdateFixtureItem(fixItem);
                }
                currMailboxID = (ushort)DecorationId.MailboxDefault;
                defaultMailbox.SetActive(true);
            }
            else
            {
                defaultMailbox.SetActive(false);
            }

            Color col;
            col = Color.white;
            col.a /= 2.0f;
            mailboxPlaceholder.GetComponentInChildren<SpriteRenderer>().color = col;
            gatePlaceholder.GetComponentInChildren<SpriteRenderer>().color = col;
            cottagePlaceholder.GetComponentInChildren<SpriteRenderer>().color = col;
            mailboxPlaceholder.SetActive(false);
            gatePlaceholder.SetActive(false);
            cottagePlaceholder.SetActive(false);

            RefreshGarden();
        }

        void OnDisable()
        {
            onTilemapSelection -= GardenTileSelected;
            EventManager.instance.EVENT_PLANT_UPDATE -= PlantUpdate;
            EventManager.instance.EVENT_WATER_PLANT -= WaterPlant;

            EventManager.instance.EVENT_INVENTORY_CUSTOMIZATION_START -= StartDecorCustomization;
            EventManager.instance.EVENT_CUSTOMIZATION_ATTEMPT -= CompleteDecorCustomization;

            EventManager.instance.EVENT_FIXTURE_CUSTOMIZATION_ATTEMPT -= CompleteFixtureCustomization;

            EventManager.instance.EVENT_TOGGLE_CUSTOMIZATION_MODE -= ToggleCustomizationMode;

            EventManager.instance.EVENT_TOGGLE_CULTIVISION_MODE -= ToggleCultivisionMode;

            EventManager.instance.EVENT_CHANGE_GARDEN_INDEX -= ChangeCustomizationCameraTarget;

            gardenManager.timers[areaIndex].Pause();
            gardenManager.timers[areaIndex].onTick -= IncrementTimer;
        }

        public override void Deactivate()
        {
            gardenVFX.Clear();
            base.Deactivate();
        }

        #region Input

        public override void ProcessAreaInput(InteractionEventData eventData)
        {
            if (eventData.interactionState.phase == PointerState.Phase.DOWN && !m_inCustomizationMode)
            {
                AreaSelection(eventData.interactionPoint);
            }
            else if (eventData.interactionState.phase == PointerState.Phase.DRAG)
            {
                AreaDragged(eventData.interactionPoint);
            }
        }

        bool checkLastTile = false;
        DG.Tweening.Sequence lastTileTimer;
        public override void AreaSelection(Vector3 worldPoint)
        {
            worldPoint.z = 0;
            lastSelectedPosition = worldPoint;
            Vector3Int temp = lastSelectedCell;

            lastSelectedCell = tilemap.WorldToCell(worldPoint);

            lastSelectedTile = tilemap.GetTile(lastSelectedCell);

            if (checkLastTile)
            {
                if (lastSelectedCell == temp)
                {
                    return;
                }
            }

            playerController.ClearActionQueue();
            onTilemapSelection?.Invoke(lastSelectedCell);

            playerController.SetDestination(CheckPlayerDestination(lastSelectedPosition));

            // Release golem selected
            EventManager.instance.HandleEVENT_GOLEM_RELEASE_SELECTED();


            if (((lastSelectedTile as IGameTile)) != null)
            {
                playerController.QueueEntityAction(((IGameTile)lastSelectedTile).DoTileAction);

            }

            checkLastTile = true;

            if (lastTileTimer.IsActive())
                lastTileTimer.Restart();
            else
                lastTileTimer =
                DOTween.Sequence().AppendInterval(0.5f)
                .OnComplete(() => checkLastTile = false);
        }

        public Vector3 CheckPlayerDestination(Vector3 worldPoint, bool checkTool = true)
        {
            if (checkTool && playerTools.currentTool.toolIndex == 0)
                return tilemap.GetCellCenterWorld(lastSelectedCell);

            Vector2 direction = (worldPoint - playerController.GetEntityPos()).normalized;
            Vector2Int offset = Vector2Int.up;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                offset = direction.x > 0 ? Vector2Int.left : Vector2Int.right;
            }

            Vector3Int offsetCellPos = lastSelectedCell + (Vector3Int)offset;
            TileBase offsetTile = tilemap.GetTile(offsetCellPos);

            if (offsetTile == null || !tileStore[offsetTile].pathable)
            {
                if (offset == Vector2Int.up)
                {
                    offset = direction.x > 0 ? Vector2Int.right : Vector2Int.left;
                }
                else
                    offset = Vector2Int.up;
            }

            offsetCellPos = lastSelectedCell + (Vector3Int)offset;


            return (tilemap.GetCellCenterWorld(offsetCellPos) + tilemap.GetCellCenterWorld(lastSelectedCell)) / 2;
        }

        //Selects a garden tile in world space
        //Called through a Unity event as of now
        public void GardenTileSelected(Vector3Int gridPos)
        {
            playerController.QueueEntityAction(DoToolAction);
            playerTools.SetToolAction(tileStore[lastSelectedTile], lastSelectedCell, this);
        }

        void DoToolAction(EntityController entityController)
        {
            playerTools.DoCurrentToolAction();
        }

        #endregion

        #region Plants
        void PlantUpdate(int areaIndex, Vector3Int cell)
        {
            if (areaIndex == this.areaIndex)
            {
                if (!gardenManager.IsEmpty(areaIndex, cell))
                {
                    UpdateSprite(cell);
                    UpdateTile(cell);
                }

            }
        }

        public PlantState GetPlant(Vector3Int cell)
        {
            if (!gardenManager.IsEmpty(areaIndex, cell))
            {
                return gardenManager.GetPlant(areaIndex, cell);
            }
            return new PlantState();
        }

        void RefreshGarden()
        {
            //List<TileState> tileStates = gardenManager.GetTiles(areaIndex);
            List<TileState> tileStates = gardenManager.GetTiles(0);
            tileStates.AddRange(gardenManager.GetTiles(1));
            tileStates.AddRange(gardenManager.GetTiles(2));
            tileStates.AddRange(gardenManager.GetTiles(3));

            foreach (TileState tileState in tileStates)
            {
                ChangeGardenTile(tileState.cell, tileStore[tileState.tileIndex]);
                UpdateTile(tileState.cell);
            }

            //List<PlantState> plants = gardenManager.GetPlants(areaIndex);
            List<PlantState> plants = gardenManager.GetPlants(0);
            plants.AddRange(gardenManager.GetPlants(1));
            plants.AddRange(gardenManager.GetPlants(2));
            plants.AddRange(gardenManager.GetPlants(3));

            foreach (PlantState plant in plants)
            {
                InstantiatePlantPrefab(plant.cell, plant.type, plant.growthStage);
            }

            //List<DecorState> decorStates = gardenManager.GetDecor(areaIndex);
            List<DecorState> decorStates = gardenManager.GetDecor(0);
            decorStates.AddRange(gardenManager.GetDecor(1));
            decorStates.AddRange(gardenManager.GetDecor(2));
            decorStates.AddRange(gardenManager.GetDecor(3));

            foreach (DecorState decorState in decorStates)
            {
                Decor decor = CollectionsSO.LoadedInstance.GetDecor((ushort)decorState.ID);
                //if the decor item is not a fixture, then it will be part of GardenCustomizer logic

                if (!decor.isFixture)
                {
                    GardenDecorItem decorItem;
                    decorItem = gardenCustomizer.GenerateDecorItem(decorState.ID);
                    decorItem.transform.position = new Vector3(decorState.x, decorState.y, decorState.z);
                    decorItem.isFlipped = decorState.isFlipped;

                    if(decorItem.isFlipped)
                    {
                        decorItem.GetComponentInChildren<SpriteRenderer>().flipX = true;
                    }

                    decorItem.onInteraction += StartDecorCustomization;
                    decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onFlip += FlipDecorItem;
                    decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onUnFlip += UnFlipDecorItem;
                    decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onPutBack += RemoveDecorItemFromGarden;
                    decorItem.DisableInteraction();
                }
                else
                {
                    FixtureItem decorItem;
                    if (decorState.ID == DecorationId.Default)
                    {
                        //defaultCottage.SetActive(true);
                        decorItem = defaultCottage.GetComponent<FixtureItem>();
                    }
                    else if (decorState.ID == DecorationId.GateofDefault)
                    {
                        //defaultGate.SetActive(true);
                        decorItem = defaultGate.GetComponent<FixtureItem>();
                    }
                    else if (decorState.ID == DecorationId.MailboxDefault)
                    {
                        //defaultMailbox.SetActive(true);
                        decorItem = defaultMailbox.GetComponent<FixtureItem>();
                    }
                    else
                    {
                        //FixtureType f = CollectionsSO.LoadedInstance.GetFixtureType(decorState.ID);

                        decorItem = fixtureCustomizer.GenerateFixtureItem(decorState.ID);
                        decorItem.transform.position = new Vector3(decorState.x, decorState.y, decorState.z);
                        decorItem.isPlaced = true;

                        switch (decorItem.fixtureType)
                        {
                            case FixtureType.Cottage:
                                currentCottage = decorItem;
                                cottageShifted = true;

                                float amountToShiftCottageX = 0.0f;
                                float amountToShiftCottageY = 0.0f;
                                float amountToShiftCottageZ = 0.0f;
                                float amountToStretchCottageX = 0.0f;
                                float amountToStretchCottageY = 0.0f;
                                float amountToStretchCottageZ = 0.0f;
                                switch (m_CurrentFixtureItem.decorID)
                                {
                                    case DecorationId.GothicManor:
                                        amountToShiftCottageX = 0.7f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.PacificNorthwestCabin:
                                        amountToShiftCottageX = -0.3f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.JapaneseTeahouse:
                                        amountToShiftCottageX = -0.4f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.OldEnglishEstate:
                                        amountToShiftCottageX = -0.3f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.PastelTownhouse:
                                        amountToShiftCottageX = -0.3f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.MagicStonehaven:
                                        amountToShiftCottageX = 0.0f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.AcornResidence:
                                        amountToShiftCottageX = -0.4f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.SnowWhiteCottage:
                                        amountToShiftCottageX = 0.0f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                    case DecorationId.WindmillHome:
                                        amountToShiftCottageX = -0.2f;
                                        amountToShiftCottageY = 0.0f;
                                        amountToShiftCottageZ = 0.0f;
                                        amountToStretchCottageX = 0.0f;
                                        amountToStretchCottageY = 0.0f;
                                        amountToStretchCottageZ = 0.0f;
                                        break;
                                }
                                cottageShiftVec = new Vector3(amountToShiftCottageX, amountToShiftCottageY, amountToShiftCottageZ);
                                cottageStretchVec = new Vector3(amountToStretchCottageX, amountToStretchCottageY, amountToStretchCottageZ);
                                currentCottage.transform.position += cottageShiftVec;
                                currentCottage.transform.localScale += cottageStretchVec;

                                break;
                            case FixtureType.Mailbox:
                                currentMailbox = decorItem;
                                break;
                            case FixtureType.Fence:
                                currentFence = decorItem;
                                leftFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(decorItem);
                                //leftFence.GetComponent<SpriteRenderer>().sprite.texture.width = defaultFenceSprite.texture.width;
                                //leftFence.GetComponent<SpriteRenderer>().sprite.texture.height = defaultFenceSprite.texture.height;
                                rightFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(decorItem);
                                fenceShifted = true;
                                float gateWidth = currentFence.GetComponentInChildren<SpriteRenderer>().size.x;
                                float defaultGateWidth = defaultGate.GetComponentInChildren<SpriteRenderer>().size.x;
                                float amountToShift = (gateWidth - defaultGateWidth) / 2.0f;
                                float amountToStretchFenceX = 0.0f;
                                float amountToStretchFenceY = 0.0f;
                                float amountToShiftMailboxX = 0.0f;
                                float amountToShiftMailboxY = 0.0f;
                                float amountToShiftMailboxZ = 0.0f;
                                float amountToStretchMailboxX = 0.0f;
                                float amountToStretchMailboxY = 0.0f;

                                switch (currentFence.decorID)
                                {
                                    case DecorationId.WoodenTreeGate:
                                        amountToShift = -2.2f;
                                        //change sizeFence here
                                        amountToStretchFenceX = -0.1f;
                                        amountToStretchFenceY = -0.1f;
                                        amountToShiftMailboxX = 0.8f;
                                        amountToShiftMailboxY = 0.2f;
                                        amountToShiftMailboxZ = -0.2f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                    case DecorationId.FancyBlackIronGate:
                                    case DecorationId.FancyWhiteIronGate:
                                        amountToShift = 0.1f;
                                        amountToStretchFenceX = 0.0f;
                                        amountToStretchFenceY = 0.1f;
                                        amountToShiftMailboxX = 0.3f;
                                        amountToShiftMailboxY = 0.0f;
                                        amountToShiftMailboxZ = -1.0f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                    case DecorationId.FieldstoneGate:
                                        amountToShift = 0.1f;
                                        amountToStretchFenceX = 0.0f;
                                        amountToStretchFenceY = -0.3f;
                                        amountToShiftMailboxX = 0.8f;
                                        amountToShiftMailboxY = 0.1f;
                                        amountToShiftMailboxZ = -0.2f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                    case DecorationId.FarmGate:
                                        amountToShift = -4.5f;
                                        amountToStretchFenceX = -0.2f;
                                        amountToStretchFenceY = -0.2f;
                                        amountToShiftMailboxX = 1.6f;
                                        amountToShiftMailboxY = 0.0f;
                                        amountToShiftMailboxZ = -0.5f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                    case DecorationId.LatticeGate:
                                        amountToShift = -0.15f;
                                        amountToStretchFenceX = 0.0f;
                                        amountToStretchFenceY = 0.1f;
                                        amountToShiftMailboxX = 0.5f;
                                        amountToShiftMailboxY = 0.2f;
                                        amountToShiftMailboxZ = -0.2f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                    case DecorationId.SimpleWoodenGate:
                                        amountToShift = 0.0f;
                                        amountToStretchFenceX = 0.0f;
                                        amountToStretchFenceY = 0.0f;
                                        amountToShiftMailboxX = 0.8f;
                                        amountToShiftMailboxY = 0.2f;
                                        amountToShiftMailboxZ = -0.2f;
                                        amountToStretchMailboxX = 0.0f;
                                        amountToStretchMailboxY = 0.0f;
                                        break;

                                }

                                shiftVec = new Vector3(amountToShift, 0.0f, 0.0f);
                                leftFence.transform.position -= shiftVec;
                                rightFence.transform.position += shiftVec;
                                //change sizing of fences
                                sizeFence = new Vector3(amountToStretchFenceX, amountToStretchFenceY, 0.0f);
                                leftFence.transform.localScale += sizeFence;
                                rightFence.transform.localScale += sizeFence;
                                //change position of mailbox
                                mailboxShiftVec = new Vector3(amountToShiftMailboxX, amountToShiftMailboxY,
                                    amountToShiftMailboxZ);
                                mailboxLogicObject.transform.position += mailboxShiftVec;
                                //change sizing of mailbox
                                mailboxStretchVec = new Vector3(amountToStretchMailboxX, amountToStretchMailboxY, 0.0f);

                                break;
                        }

                        decorItem.onInteraction += StartFixtureCustomization;

                        if (!decorItem.isDefault)
                        {
                            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onFlip += FlipDecorItem;
                            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onUnFlip += UnFlipDecorItem;
                            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onPutBack += RemoveFixtureItem;
                        }
                    }
                    decorItem.DisableInteraction();
                }

            }
        }

        public void UpdateTile(Vector3Int cell)
        {
            if (gardenManager.IsEmpty(areaIndex, cell) || gardenManager.PlantNeedsWater(areaIndex, cell))
            {
                tilemap.SetColor(cell, gardenManager.dryTileTintColor);
            }
            else
                tilemap.SetColor(cell, gardenManager.wateredTileTintColor);
        }

        public void UpdateSprite(Vector3Int cell)
        {
            if (!plantPrefabLookup.ContainsKey(cell)) return;

            PlantId id = gardenManager.GetPlantType(areaIndex, cell);
            Transform plantTransform = plantPrefabLookup[cell].transform;
            PlantState plant = gardenManager.GetPlant(areaIndex, cell);
            Genotype genotype = gardenManager.GetGenotype(areaIndex, cell);

            SpriteRenderer spriteRenderer = plantTransform.Find("Sprite3D").GetComponent<SpriteRenderer>();
            if (CollectionsSO.IsFlower(id))
            {
                spriteRenderer.sprite = CollectionsSO.LoadedInstance.GetSprite(plant.type, genotype, plant.growthStage);
                plantTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * plant.genotype.SpriteSize();
            }
            else if (CollectionsSO.IsVegetable(id))
            {
                spriteRenderer.sprite = CollectionsSO.LoadedInstance.GetSprite(plant.type, genotype, plant.growthStage);
                plantTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * plant.genotype.SpriteSize();
            }
            else if (CollectionsSO.IsFruit(id))
            {
                spriteRenderer.sprite = CollectionsSO.LoadedInstance.GetFruitTree(plant.type, genotype, plant.growthStage);
                plantTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }

            if (gardenManager.PlantIsWilted(areaIndex, cell))
            {
                spriteRenderer.color = new Color(0.71f, 0.4f, 0.11f);
                gardenVFX.PlayDryParticle(tilemap.GetCellCenterWorld(cell));
                onPlantWilted?.Invoke(cell);
            }
            else if (gardenManager.PlantIsDead(areaIndex, cell))
            {
                gardenVFX.StopDryParticle(tilemap.GetCellCenterWorld(cell));
                spriteRenderer.color = new Color(1f, 1f, 1f);
                if (CollectionsSO.IsFlower(id)) spriteRenderer.sprite = Resources.Load<Sprite>("Flowers/DeadPlant/PLA_DeadFlower");
                else if (CollectionsSO.IsVegetable(id)) spriteRenderer.sprite = Resources.Load<Sprite>("Flowers/DeadPlant/PLA_DeadVegetable");
                else spriteRenderer.sprite = Resources.Load<Sprite>("Flowers/DeadPlant/PLA_DeadFruit");

                plantTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * plant.genotype.SpriteSize();
                onPlantDead?.Invoke(cell);
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f);
            }
        }

        public void InstantiatePlantPrefab(Vector3Int cell, PlantId id, int growthStage)
        {
            Vector3 centerOfCell = tilemap.GetCellCenterWorld(cell);
            if (cell.y % 2 == 0)
                centerOfCell += plantRowHorizontalOffset * Vector3.right;
            else
                centerOfCell += plantRowHorizontalOffset * Vector3.left;
            plantPrefabLookup[cell] = Instantiate(defaultPlantPrefab, centerOfCell, Quaternion.identity);
            UpdateSprite(cell);
            UpdateTile(cell);
        }

        public void DestroyPlantPrefab(Vector3Int cell)
        {
            if (plantPrefabLookup.ContainsKey(cell))
            {
                Destroy(plantPrefabLookup[cell]);
                plantPrefabLookup.Remove(cell);

                tilemap.SetColor(cell, gardenManager.dryTileTintColor);
            }
        }

        public void CreatePlant(PlantId seed, Genotype genotype, Vector3Int cell, int growthStage = 0)
        {
            if (!plantPrefabLookup.ContainsKey(cell) || gardenManager.IsEmpty(areaIndex, cell))
            {
                gardenManager.CreatePlant(seed, genotype, growthStage, areaIndex, cell);
                InstantiatePlantPrefab(cell, seed, growthStage);
                // Remove the seed from inventory.

            }
        }

        public void IncrementTimer(int value)
        {
            // Get an initial plantList (with their original timers) and increment all waterTimers
            // in that list
            List<PlantState> plantsToUpdate = gardenManager.GetPlants(areaIndex);
            foreach (PlantState plant in plantsToUpdate)
            {
                gardenManager.IncrementWaterTimer(areaIndex, plant.cell, value);
            }
            PlayParticles();
            returnFromPause = false;

            onGardenTick?.Invoke();
        }

        public void PlayParticles()
        {
            List<PlantState> updatedPlantsList = gardenManager.GetPlants(areaIndex);

            if (!returnFromPause)
            {
                foreach (PlantState updatedPlant in updatedPlantsList)
                {
                    if (updatedPlant.waterStage == 1 && updatedPlant.waterTimer >= CollectionsSO.LoadedInstance.GetPlant(updatedPlant.type).growthTime)
                    {
                        if (gardenManager.UpdateGrowthStage(areaIndex, updatedPlant.cell))
                        {
                            if (gardenManager.PlantIsFullyGrown(areaIndex, updatedPlant.cell))
                                gardenVFX.PlayGrowthParticle(tilemap.GetCellCenterWorld(updatedPlant.cell));
                        }
                    }
                    else
                    {
                        if (updatedPlant.waterTimer == CollectionsSO.LoadedInstance.GetPlant(updatedPlant.type).growthTime)
                        {
                            Debug.Log("Plant is ready for water");
                        }
                        else if (updatedPlant.waterTimer == gardenManager.WiltTime // Plant has reached its wilt time while player is playing
                            || (gardenManager.PlantIsWilted(areaIndex, updatedPlant.cell) && returnFromPause)) // Plant has reached wilt time while player is away
                        {
                            // Wilted
                            wiltedPlantList.Add(updatedPlant);
                            gardenVFX.PlayDryParticle(tilemap.GetCellCenterWorld(updatedPlant.cell));
                        }
                        else if (!gardenManager.PlantIsFullyGrown(areaIndex, updatedPlant.cell))
                        {
                            if (updatedPlant.waterTimer == gardenManager.DeathTime // Plant has reached death time while player is playing
                                || (gardenManager.PlantIsDead(areaIndex, updatedPlant.cell) && returnFromPause)) // Plant has reached death time while player is away
                            {
                                // Dead and remove dead plant from wiltedPlantList
                                wiltedPlantList.RemoveAll(item => item.cell == updatedPlant.cell);
                                gardenVFX.StopDryParticle(tilemap.GetCellCenterWorld(updatedPlant.cell));
                            }
                        }
                    }
                    UpdateSprite(updatedPlant.cell);
                    UpdateTile(updatedPlant.cell);
                }
            }
            else
            {
                foreach (PlantState plantOnReturn in updatedPlantsList)
                {
                    if (!plantOnReturn.previouslyDead)
                    {
                        wiltedPlantList.Add(plantOnReturn);
                        gardenVFX.PlayDryParticle(tilemap.GetCellCenterWorld(plantOnReturn.cell));
                    }

                }
            }

            if(wiltedPlantList.Count >= 5)
            {
                EventManager.instance.HandleEVENT_WILT_ACHIVEMENT(1);
            }

        }

        public void WaterPlant(Vector3Int cell)
        {
            if (!gardenManager.PlantIsDead(areaIndex, cell))
            {
                if (gardenManager.UpdateWaterStage(areaIndex, cell))
                {
                    gardenManager.UpdateGrowthStage(areaIndex, cell);
                    // Debug.Log("Plant Growth via WaterPlant");
                }

                foreach (PlantState plantToRemove in wiltedPlantList)
                {
                    if (plantToRemove.cell == cell)
                    {
                        wiltedPlantList.Remove(plantToRemove);
                        gardenVFX.StopDryParticle(tilemap.GetCellCenterWorld(cell));
                        break;
                    }
                }
                UpdateSprite(cell);
                UpdateTile(cell);
            }

            // Spawn a particle system so we know that a plant has been watered
            gardenVFX.PlayWaterParticle(tilemap.GetCellCenterWorld(cell));
        }

        Dictionary<Vector3Int, Sequence> waterAnimationLookup = new Dictionary<Vector3Int, Sequence>();
        public void WaterTile(Vector3Int cell)
        {
            gardenVFX.PlayWaterParticle(tilemap.GetCellCenterWorld(cell));

            if (!waterAnimationLookup.TryAdd(cell, null))
                waterAnimationLookup[cell].Kill(false);

            tilemap.SetColor(cell, gardenManager.wateredTileTintColor);

            Color lerpColor = gardenManager.wateredTileTintColor;
            waterAnimationLookup[cell] =
            DOTween.Sequence()
            .AppendInterval(1.0f)
            .SetEase(Ease.OutBack)
            .OnUpdate(
                () => LerpCellColor(cell))

            .OnComplete(
                () => waterAnimationLookup.Remove(cell));
        }

        void LerpCellColor(Vector3Int cell)
        {
            Color lerpColor = Color.Lerp(
                    gardenManager.wateredTileTintColor,
                    gardenManager.dryTileTintColor,
                    waterAnimationLookup[cell].Elapsed() / waterAnimationLookup[cell].Duration());

            tilemap.SetColor(cell, lerpColor);
        }

        public void OnReturnWaterPlant(Vector3Int cell)
        {
            if (gardenManager.UpdateWaterStage(areaIndex, cell))
            {
                gardenManager.UpdateGrowthStage(areaIndex, cell);
                // Debug.Log("Plant Growth via WaterPlant");
            }

            foreach (PlantState plantToRemove in wiltedPlantList)
            {
                if (plantToRemove.cell == cell)
                {
                    wiltedPlantList.Remove(plantToRemove);


                    PlantState updatedPlant = gardenManager.GetPlant(areaIndex, cell);
                    if (updatedPlant.waterTimer >= gardenManager.WiltTime)
                    {
                        wiltedPlantList.Add(updatedPlant);
                    }
                    else
                        gardenVFX.StopDryParticle(tilemap.GetCellCenterWorld(cell));

                    break;
                }
            }
            UpdateSprite(cell);
            UpdateTile(cell);
        }

        public void FertilizePlant(Vector3Int cell)
        {
            if (gardenManager.SetFertilization(areaIndex, cell))
            {
                //Spawn a particle system so we know that a plant has been fertilized
                //gardenVFX.PlayFertilizerParticle(tilemap.GetCellCenterWorld(cell));
            }
        }

        public List<PlantState> GetNeighbors(Vector3Int cell)
        {
            return gardenManager.GetNeighbors(areaIndex, cell);
        }

        public List<PlantState> GetBreedingCandidates(Vector3Int cell)
        {
            return gardenManager.GetBreedingCandidates(areaIndex, cell);
        }

        public List<PlantState> GetCultivisionCandidates(Vector3Int cell)
        {
            List<PlantState> breedingCandidates = gardenManager.GetBreedingCandidates(areaIndex, cell);
            List<PlantState> candidates = gardenManager.GetWiltedNeighbors(areaIndex, cell);
            for (int i = 0; i < breedingCandidates.Count; i++)
            {
                candidates.Add(breedingCandidates[i]);
            }
            return candidates;
        }

        public bool HarvestPlant(Vector3Int cell)
        {
            if (gardenManager.IsEmpty(areaIndex, cell)) return false;

            gardenVFX.StopDryParticle(tilemap.GetCellCenterWorld(cell));

            int maxGrowthStages = CollectionsSO.LoadedInstance.GetPlant(
                gardenManager.GetPlantType(areaIndex, cell))
                .growthStages;

            string debug = "Breeding Candidate Debug (Click to Expand)\n";

            if (gardenManager.PlantIsBreedable(areaIndex, cell))
            {
                debug += "Breeding candidates: ";
                List<PlantState> breedingCandidates = GetBreedingCandidates(cell);
                foreach (PlantState neighbor in breedingCandidates)
                {
                    debug += neighbor.genotype + " ";
                }

                PlantId motherPlantType = gardenManager.GetPlantType(areaIndex, cell);
                Genotype motherGenotype = gardenManager.GetGenotype(areaIndex, cell);
                PlantId childPlantType = motherPlantType;
                Genotype childGenotype = motherGenotype;

                if (breedingCandidates.Count != 0)
                {
                    Genotype fatherGenotype = breedingCandidates[UnityEngine.Random.Range(0, breedingCandidates.Count)].genotype;
                    childGenotype = motherGenotype.Cross(fatherGenotype);
                    EventManager.instance.HandleEVENT_PLANT_CROSSBREED((ushort)motherPlantType);
                    debug += "\nBreeding occured.";
                }
                else
                    debug += "\nNo breeding occured.";

                Debug.Log(debug);

                PlantProperties properties = CollectionsSO.LoadedInstance.GetPlant(childPlantType);
                EventManager.instance.HandleEVENT_INVENTORY_ADD_PLANT((ushort)motherPlantType, motherGenotype);

                float trueDropRate = DropRate;
                if (GetPlant(cell).isFertilized) trueDropRate += FertilizationBonus;
                if (UnityEngine.Random.Range(0f, 1f) <= trueDropRate / 100)
                {
                    EventManager.instance.HandleEVENT_INVENTORY_ADD_SEED((ushort)childPlantType, childGenotype);
                }

                DestroyPlant(cell);

                CharacterId golemType;
                if (CollectionsSO.LoadedInstance.PlantToGolem(motherPlantType, out golemType)
                    && golemManager.IsSpawnable((ushort)golemType))
                {
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);
                    EventManager.instance.HandleEVENT_GOLEM_SPAWN((ushort)golemType, cellCenter);
                }
                return true;
            }
            else if (gardenManager.PlantIsWilted(areaIndex, cell))
            {
                
                wiltedPlantList.RemoveAll(item => item.cell == cell);
            }

            DestroyPlant(cell);
            return false;
        }

        public void DestroyPlant(Vector3Int cell)
        {
            DestroyPlantPrefab(cell);
            gardenManager.DestroyPlant(areaIndex, cell);
        }

        [ContextMenu("ClearPlants")]
        public void ClearPlants()
        {
            foreach (Vector3Int cell in plantPrefabLookup.Keys)
            {
                Destroy(plantPrefabLookup[cell]);
            }
            plantPrefabLookup = new Dictionary<Vector3Int, GameObject>();
            gardenManager.ClearGarden(areaIndex);
        }

        [ContextMenu("ResetGarden")]
        public void ResetGarden()
        {
            Vector3Int playableTopLeft = new Vector3Int(-8, 4, 0);
            Vector3Int playableBottomRight = new Vector3Int(6, -5, 0);
            for (int x = -8; x <= 6; x++)
            {
                for (int y = -5; y <= 4; y++)
                {
                    ChangeGardenTileToGrass(new Vector3Int(x, y, 0));
                }
            }
            ClearPlants();
        }

        [ContextMenu("InspectPlants")]
        public void InspectPlants()
        {
            plantListDebug = gardenManager.GetPlants(areaIndex);
        }

        [ContextMenu("ToggleGrowthTimer")]
        public void ToggleGrowthTimer()
        {
            if (!gardenManager.timers[areaIndex].paused)
            {
                gardenManager.timers[areaIndex].Pause();
                Debug.Log("Paused Growth Timer");
            }
            else
            {
                gardenManager.timers[areaIndex].Resume();
                Debug.Log("Resumed Growth Timer");
            }
        }

        [ContextMenu("FlowerTest")]
        public void FlowerTest()
        {
            ResetGarden();
            Vector3Int topLeft = Vector3Int.zero + 3 * Vector3Int.up + 7 * Vector3Int.left;
            int row = 0;
            foreach (FlowerId flowerId in System.Enum.GetValues(typeof(FlowerId)))
            {
                Vector3Int leftTile = topLeft + row * Vector3Int.down;
                Vector3Int right = Vector3Int.right;
                for (int j = 0; j < 7; j++)
                {
                    ChangeGardenTileToPlot_Occupied(leftTile + j * right);
                }
                PlantId plantId = (PlantId)flowerId;
                Genotype.Generation mega = Genotype.Generation.F2;
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 0, 0);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 1, 1);
                CreatePlant(plantId, new Genotype("aabb"), leftTile + right * 2, 2);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 3, 2);
                CreatePlant(plantId, new Genotype("AABB"), leftTile + right * 4, 2);
                CreatePlant(plantId, new Genotype("aabb", mega), leftTile + right * 5, 2);
                CreatePlant(plantId, new Genotype("AABB", mega), leftTile + right * 6, 2);
                row++;
            }
        }

        [ContextMenu("VegetableTest")]
        public void VegetableTest()
        {
            ResetGarden();
            Vector3Int topLeft = Vector3Int.zero + 3 * Vector3Int.up + 7 * Vector3Int.left;
            int row = 0;
            foreach (VegetableId vegId in System.Enum.GetValues(typeof(VegetableId)))
            {
                Vector3Int leftTile = topLeft + row * Vector3Int.down;
                Vector3Int right = Vector3Int.right;
                for (int j = 0; j < 7; j++)
                {
                    ChangeGardenTileToPlot_Occupied(leftTile + j * right);
                }
                PlantId plantId = (PlantId)vegId;
                Genotype.Generation mega = Genotype.Generation.F2;
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 0, 0);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 1, 1);
                CreatePlant(plantId, new Genotype("aabb"), leftTile + right * 2, 2);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 3, 2);
                CreatePlant(plantId, new Genotype("AABB"), leftTile + right * 4, 2);
                CreatePlant(plantId, new Genotype("aabb", mega), leftTile + right * 5, 2);
                CreatePlant(plantId, new Genotype("AABB", mega), leftTile + right * 6, 2);
                row++;
            }
        }

        [ContextMenu("FruitTest")]
        public void FruitTest()
        {
            ResetGarden();
            Vector3Int topLeft = tilemap.WorldToCell(transform.position) + 3 * Vector3Int.up + 7 * Vector3Int.left;
            int row = 0;
            foreach (FruitId fruitId in System.Enum.GetValues(typeof(FruitId)))
            {
                Vector3Int leftTile = topLeft + row * Vector3Int.down;
                Vector3Int right = Vector3Int.right;
                for (int j = 0; j < 7; j++)
                {
                    ChangeGardenTileToPlot_Occupied(leftTile + j * right);
                }
                PlantId plantId = (PlantId)fruitId;
                Genotype.Generation mega = Genotype.Generation.F2;
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 0, 0);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 1, 1);
                CreatePlant(plantId, new Genotype("aabb"), leftTile + right * 2, 2);
                CreatePlant(plantId, new Genotype("AaBb"), leftTile + right * 3, 2);
                CreatePlant(plantId, new Genotype("AABB"), leftTile + right * 4, 2);
                CreatePlant(plantId, new Genotype("aabb", mega), leftTile + right * 5, 2);
                CreatePlant(plantId, new Genotype("AABB", mega), leftTile + right * 6, 2);
                row++;
            }
        }

        [ContextMenu("GenotypeSpriteTest")]
        public void GenotypeSpriteTest()
        {
            ResetGarden();
            Vector3Int right = Vector3Int.zero + 3 * Vector3Int.up;
            for (int i = 0; i <= 8; i += 2)
            {
                DestroyPlant(right + i * Vector3Int.down);
                ChangeGardenTileToPlot_Occupied(right + i * Vector3Int.down);
            }
            CreatePlant(PlantId.Rose, new Genotype("aabb"), right + 0 * Vector3Int.down, 2);
            CreatePlant(PlantId.Rose, new Genotype("AaBb"), right + 2 * Vector3Int.down, 2);
            CreatePlant(PlantId.Rose, new Genotype("AABB"), right + 4 * Vector3Int.down, 2);
            // Megas
            CreatePlant(PlantId.Rose, new Genotype("Aabb", Genotype.Generation.F2), right + 6 * Vector3Int.down, 2);
            CreatePlant(PlantId.Rose, new Genotype("AaBB", Genotype.Generation.F2), right + 8 * Vector3Int.down, 2);
        }

        [ContextMenu("CrossBreedTest")]
        public void CrossBreedTest()
        {
            Genotype first;
            Genotype second;

            Debug.Log("child is p1");
            first = new Genotype("aaBb");
            second = new Genotype("aaBb");
            first.Cross(second);

            Debug.Log("if both parents are f2, child is f2");
            first = new Genotype("aaBb", Genotype.Generation.F2);
            second = new Genotype("aaBb", Genotype.Generation.F2);
            first.Cross(second);

            Debug.Log("if parents are duplicate homozygous, child is F1");
            first = new Genotype("aabb");
            second = new Genotype("aabb");
            first.Cross(second);

            first = new Genotype("aaBB");
            second = new Genotype("aaBB");
            first.Cross(second);

            Debug.Log("if parents are duplicate homozygous and both f1, child is f2");
            first = new Genotype("aabb", Genotype.Generation.F1);
            second = new Genotype("aabb", Genotype.Generation.F1);
            first.Cross(second);

            first = new Genotype("aaBB", Genotype.Generation.F1);
            second = new Genotype("aaBB", Genotype.Generation.F1);
            first.Cross(second);

            Debug.Log("if parents are duplicate homozygous and one is f1 and one is f2, child is f2");
            first = new Genotype("aabb", Genotype.Generation.F2);
            second = new Genotype("aabb", Genotype.Generation.F1);
            first.Cross(second);

            first = new Genotype("aaBB", Genotype.Generation.F2);
            second = new Genotype("aaBB", Genotype.Generation.F1);
            first.Cross(second);
        }

        [ContextMenu("IdTypeTest")]
        public void IdTypeTest()
        {
            Debug.Log("1001 is flower " + CollectionsSO.IsFlower((PlantId)1001));
            Debug.Log("1018 is flower " + CollectionsSO.IsFlower((PlantId)1018));
            Debug.Log("1008 is vegetable " + CollectionsSO.IsVegetable((PlantId)1008));
            Debug.Log("1001 is vegetable " + CollectionsSO.IsVegetable((PlantId)1001));
            Debug.Log("1015 is fruit " + CollectionsSO.IsFruit((PlantId)1015));
            Debug.Log("1008 is fruit " + CollectionsSO.IsFruit((PlantId)1008));
        }

        [ContextMenu("ExtensionSaverTest")]
        public void ExtensionSaverTest()
        {
            Debug.Log("AreaIndex " + areaIndex);
            Debug.Log("Saving Plant");
            gardenManager.CreatePlant(PlantId.Rose, new Genotype("aabb"), 0, areaIndex, Vector3Int.zero);
        }
        #endregion

        #region Tiles

        public void ChangeGardenTile(Vector3Int position, TileData newTile)
        {
            TileData prev = tileStore[tilemap.GetTile(position)];

            if (prev.fertilized && !newTile.fertilized)
                gardenVFX.StopFertilizerParticle(tilemap.GetCellCenterWorld(position));
            else if (newTile.fertilized)
                gardenVFX.PlayFertilizerParticle(tilemap.GetCellCenterWorld(position));

            tilemap.SetTile(position, newTile.tile);
            gardenManager.UpdateGardenTile(areaIndex, position, tileStore.tileDataSet.IndexOf(newTile));

        }

        public void ChangeGardenTileToGrass(Vector3Int position)
        {
            ChangeGardenTile(position, tileStore[0]);
        }

        public void ChangeGardenTileToPlot_Empty(Vector3Int position)
        {
            ChangeGardenTile(position, tileStore[1]);
            tilemap.SetColor(position, gardenManager.dryTileTintColor);
        }

        public void ChangeGardenTileToPlot_Occupied(Vector3Int position)
        {
            ChangeGardenTile(position, tileStore[2]);
            tilemap.SetColor(position, gardenManager.dryTileTintColor);
        }

        public void ChangeGardenTileToPlot_Fertilized(Vector3Int position)
        {
            ChangeGardenTile(position, tileStore[5]);
            tilemap.SetColor(position, gardenManager.dryTileTintColor);
        }

        public void ChangeOccupiedGardenTileTo_Fertilized(Vector3Int position)
        {
            ChangeGardenTile(position, tileStore[6]);
        }

        #endregion

        #region  Customization

        GardenDecorItem m_CurrentDecorItem;
        FixtureItem m_CurrentFixtureItem;
        bool m_IsGeneratedDecorItem = false;
        bool m_IsGeneratedFixtureItem = false;
        Vector3 m_OriginalPosition;

        public void ToggleCustomizationMode()
        {
            if (m_inCustomizationMode) ExitCustomizationMode();
            else EnterCustomizationMode();
        }

        public void ChangeCustomizationCameraTarget(int index)
        {
            areaIndex = index;
            switch(index)
            {
                case 0: //base
                    customizationCamera.m_Follow = BaseGardenCameraTarget.transform;
                    break;
                case 1: //west
                    customizationCamera.m_Follow = WestGardenCameraTarget.transform;
                    break;
                case 2: //east
                    customizationCamera.m_Follow = EastGardenCameraTarget.transform;
                    break;
                case 3: //north
                    customizationCamera.m_Follow = NorthGardenCameraTarget.transform;
                    break;
            }
        }

        public void EnterCustomizationMode()
        {
            openCustomizationSFX.Play();
            m_inCustomizationMode = true;
            customizationCamera.gameObject.SetActive(true);
            playerController.entity.StopActions();
            playerController.entity.gameObject.SetActive(false);
            EventManager.instance.HandleEVENT_GOLEM_DISABLE();
            customizationCanvas.SetActive(true);
            //make mailbox UI disabled
            mailboxLogic.SetActive(false);
            //GameObject.Find("Mailbox").GetComponent<MailboxUIDisplay>().inCustomization = true;
        }

        public void ExitCustomizationMode()
        {
            closeCustomizationSFX.Play();
            m_inCustomizationMode = false;
            customizationCamera.gameObject.SetActive(false);
            playerController.entity.gameObject.SetActive(true);
            EventManager.instance.HandleEVENT_GOLEM_ENABLE();

            customizationCanvas.SetActive(false);
            //make mailbox UI enabled
            mailboxLogic.SetActive(true);
            //GameObject.Find("Mailbox").GetComponent<MailboxUIDisplay>().inCustomization = false;
        }

        public void StartDecorCustomization(IInventoryItem item)
        {
            Decor decor = CollectionsSO.LoadedInstance.GetDecor(item.itemID);
            if (!decor.isFixture)
            {
                GardenDecorItem decorItem = gardenCustomizer.GenerateDecorItem((DecorationId)item.itemID);
                m_IsGeneratedDecorItem = true;

                if (!m_inCustomizationMode) EventManager.instance.HandleEVENT_TOGGLE_CUSTOMIZATION_MODE();

                StartDecorCustomization(decorItem);
            }
            else
            {
                FixtureItem fixtureItem = fixtureCustomizer.GenerateFixtureItem((DecorationId)item.itemID);
                m_IsGeneratedFixtureItem = true;

                if (!m_inCustomizationMode) EventManager.instance.HandleEVENT_TOGGLE_CUSTOMIZATION_MODE();

                StartFixtureCustomization(fixtureItem);
            }
        }

        public void StartDecorCustomization(GardenDecorItem decorItem)
        {
            m_CurrentDecorItem = decorItem;
            m_OriginalPosition = m_CurrentDecorItem.transform.position;
            lastDraggedPosition = m_OriginalPosition;
            gardenCustomizer.EnterDecorCustomizationState(this, m_CurrentDecorItem);
        }

        public void CompleteDecorCustomization(bool successful)
        {
            if (successful)
            {
                successfulPlacementSFX.Play();
                if (m_IsGeneratedDecorItem)
                {
                    AddGardenDecorItem(m_CurrentDecorItem);
                }
                else
                {
                    UpdateGardenDecorItem(m_CurrentDecorItem);
                }
            }
            else
            {
                unsuccessfulPlacementSFX.Play();
                if (m_IsGeneratedDecorItem)
                {
                    Destroy(m_CurrentDecorItem.gameObject);
                }
                else
                    m_CurrentDecorItem.transform.position = m_OriginalPosition;
            }

            m_IsGeneratedDecorItem = false;
        }
        
        public void AddGardenDecorItem(GardenDecorItem decorItem)
        {
            gardenManager.UpdateDecorItem(areaIndex, decorItem.decorID, decorItem.isFlipped, decorItem.transform.position);
            EventManager.instance.HandleEVENT_INVENTORY_REMOVE_DECOR((ushort)decorItem.decorID);

            decorItem.onInteraction += StartDecorCustomization;
            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onFlip += FlipDecorItem;
            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onUnFlip += UnFlipDecorItem;
            decorItem.GetComponentInChildren<DecorMenuUIDisplay>().onPutBack += RemoveDecorItemFromGarden;
        }

        public void UpdateGardenDecorItem(GardenDecorItem decorItem)
        {
            gardenManager.UpdateDecorItem(areaIndex, decorItem.decorID, decorItem.isFlipped, decorItem.transform.position, m_OriginalPosition);
        }

        public void RemoveDecorItemFromGarden(GardenDecorItem decorItem)
        {
            putInInventorySFX.Play();
            Debug.Log("Decor item removed!");
            gardenManager.RemoveDecorItem(areaIndex, decorItem.decorID, decorItem.transform.position);
            EventManager.instance.HandleEVENT_INVENTORY_ADD_DECOR((ushort)decorItem.decorID);
            Destroy(decorItem.gameObject);
        }

        public void FlipDecorItem(GardenDecorItem item)
        {
            item.isFlipped = true;
            UpdateGardenDecorItem(item);
            item.GetComponentInChildren<SpriteRenderer>().flipX = true;
            flip1DecorSFX.Play();
        }

        public void UnFlipDecorItem(GardenDecorItem item)
        {
            item.isFlipped = false;
            UpdateGardenDecorItem(item);
            item.GetComponentInChildren<SpriteRenderer>().flipX = false;
            flip2DecorSFX.Play();
        }
        
        //FIXTURES:

        /// <summary>
        /// Given a fence gate, return the fence sprite matching the gate
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private Sprite GetFenceSprite(FixtureItem f)
        {
            if (f.isDefault)
            {
                return defaultFenceSprite;
            }
            else
            {
                string s = CollectionsSO.LoadedInstance.GetItem((ushort)f.decorID).spritePath;
                int ind = s.LastIndexOf("_");
                s = s.Remove(ind); //removes "_Gate" from string
                return CollectionsSO.LoadedInstance.GetSprite(s);
            }
        }

        public void StartFixtureCustomization(FixtureItem fixtureItem)
        {
            if (!fixtureItem.isDefault && !fixtureItem.isPlaced)
            {
                m_CurrentFixtureItem = fixtureItem;
                m_CurrentFixtureItem.isDefault = false;
                m_OriginalPosition = m_CurrentFixtureItem.transform.position;
                lastDraggedPosition = m_OriginalPosition;
                fixtureCustomizer.EnterFixtureCustomizationState(this, m_CurrentFixtureItem);
            }
        }

        public void CompleteFixtureCustomization(bool successful)
        {
            if (successful)
            {
                successfulPlacementSFX.Play();
                mailboxPlaceholder.SetActive(false);
                gatePlaceholder.SetActive(false);
                cottagePlaceholder.SetActive(false);
                if (m_IsGeneratedFixtureItem)
                {

                    //if we successfully place a NEW fixture item, then we need to remove the previous fixture item at that slot and move that to inventory

                    //TODO:
                    //check if is default, if so then ignore switch
                    //hide default/unhide default instead of destroy
                    //need to set the currentMailbox, current etc to the new current

                    switch (m_CurrentFixtureItem.fixtureType)
                    {
                        case FixtureType.Mailbox:
                            gardenManager.RemoveDecorItem(areaIndex, (DecorationId)currentMailbox.decorID, currentMailbox.transform.position);
                            if (!currentMailbox.isDefault)
                            {
                                EventManager.instance.HandleEVENT_INVENTORY_ADD_DECOR((ushort)currentMailbox.decorID);
                                Destroy(currentMailbox.gameObject);
                            }
                            else
                            {
                                currentMailbox.gameObject.SetActive(false);
                            }
                            currentMailbox = m_CurrentFixtureItem;
                            currMailboxID = (ushort)m_CurrentFixtureItem.decorID;
                            break;
                        case FixtureType.Cottage:
                            gardenManager.RemoveDecorItem(areaIndex, (DecorationId)currentCottage.decorID, currentCottage.transform.position);
                            if (!currentCottage.isDefault)
                            {
                                EventManager.instance.HandleEVENT_INVENTORY_ADD_DECOR((ushort)currentCottage.decorID);
                                Destroy(currentCottage.gameObject);
                                currentCottage.transform.position -= cottageShiftVec;
                                currentCottage.transform.localScale -= cottageStretchVec;
                            }
                            else
                            {
                                currentCottage.gameObject.SetActive(false);
                            }
                            currentCottage = m_CurrentFixtureItem;
                            currCottageID = (ushort)m_CurrentFixtureItem.decorID;
                            float amountToShiftCottageX = 0.0f;
                            float amountToShiftCottageY = 0.0f;
                            float amountToShiftCottageZ = 0.0f;
                            float amountToStretchCottageX = 0.0f;
                            float amountToStretchCottageY = 0.0f;
                            float amountToStretchCottageZ = 0.0f;
                            switch(m_CurrentFixtureItem.decorID)
                            {
                                case DecorationId.GothicManor:
                                    amountToShiftCottageX = 0.7f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.PacificNorthwestCabin:
                                    amountToShiftCottageX = -0.3f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.JapaneseTeahouse:
                                    amountToShiftCottageX = -0.4f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.OldEnglishEstate:
                                    amountToShiftCottageX = -0.3f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.PastelTownhouse:
                                    amountToShiftCottageX = -0.3f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.MagicStonehaven:
                                    amountToShiftCottageX = 0.0f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.AcornResidence:
                                    amountToShiftCottageX = -0.4f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.SnowWhiteCottage:
                                    amountToShiftCottageX = 0.0f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                                case DecorationId.WindmillHome:
                                    amountToShiftCottageX = -0.2f;
                                    amountToShiftCottageY = 0.0f;
                                    amountToShiftCottageZ = 0.0f;
                                    amountToStretchCottageX = 0.0f;
                                    amountToStretchCottageY = 0.0f;
                                    amountToStretchCottageZ = 0.0f;
                                    break;
                            }
                            cottageShiftVec = new Vector3(amountToShiftCottageX, amountToShiftCottageY, amountToShiftCottageZ);
                            cottageStretchVec = new Vector3(amountToStretchCottageX, amountToStretchCottageY, amountToStretchCottageZ);
                            currentCottage.transform.position += cottageShiftVec;
                            currentCottage.transform.localScale += cottageStretchVec;
                            gardenManager.UpdateDecorItem(areaIndex, currentCottage.decorID, currentCottage.isFlipped, currentCottage.transform.position);
                            cottageShifted = true;

                            break;
                        case FixtureType.Fence:
                            gardenManager.RemoveDecorItem(areaIndex, (DecorationId)currentFence.decorID, currentFence.transform.position);
                            if (!currentFence.isDefault)
                            {
                                EventManager.instance.HandleEVENT_INVENTORY_ADD_DECOR((ushort)currentFence.decorID);
                                Destroy(currentFence.gameObject);
                                leftFence.transform.position += shiftVec;
                                rightFence.transform.position -= shiftVec;
                                //change scale of fence back to normal
                                leftFence.transform.localScale -= sizeFence;
                                rightFence.transform.localScale -= sizeFence;
                                currentMailbox.transform.position -= mailboxShiftVec;
                                mailboxLogicObject.transform.position -= mailboxShiftVec;
                            }
                            else
                            {
                                currentFence.gameObject.SetActive(false);
                            }
                            currentFence = m_CurrentFixtureItem;
                            currGateID = (ushort)m_CurrentFixtureItem.decorID;
                            //set the left and right fences
                            leftFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(currentFence);
                            rightFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(currentFence);
                            /*compare sizes of gates sprites -- if wider than default, shift left and right fences over by
                             (currentgate.width - defaultgate.width)/2
                            */
                            //float gateWidth = currentFence.GetComponentInChildren<SpriteRenderer>().size.x;
                            //float defaultGateWidth = defaultGate.GetComponentInChildren<SpriteRenderer>().size.x;
                            //float amountToShift = ((gateWidth - defaultGateWidth) / 2.0f) + 0.5f;

                            float amountToShift = 0.0f; //to shift the gate
                            float amountToStretchFenceX = 0.0f;
                            float amountToStretchFenceY = 0.0f;
                            float amountToShiftMailboxX = 0.0f;
                            float amountToShiftMailboxY = 0.0f;
                            float amountToShiftMailboxZ = 0.0f;
                            float amountToStretchMailboxX = 0.0f;
                            float amountToStretchMailboxY = 0.0f;

                            switch (m_CurrentFixtureItem.decorID)
                            {
                                case DecorationId.WoodenTreeGate:
                                    shiftGate = new Vector3(0.3f, 0.1f, 0.0f); //want to shift wooden tree gate to the left a bit and a little down
                                    amountToShift = -2.2f;
                                    //change sizeFence here
                                    amountToStretchFenceX = -0.1f;
                                    amountToStretchFenceY = -0.1f;
                                    amountToShiftMailboxX = 0.8f;
                                    amountToShiftMailboxY = 0.2f;
                                    amountToShiftMailboxZ = -0.2f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    break;

                                case DecorationId.FancyBlackIronGate:
                                case DecorationId.FancyWhiteIronGate:
                                    amountToShift = 0.1f;
                                    amountToStretchFenceX = 0.0f;
                                    amountToStretchFenceY = 0.1f;
                                    amountToShiftMailboxX = 0.3f;
                                    amountToShiftMailboxY = 0.0f;
                                    amountToShiftMailboxZ = -1.0f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    shiftGate = new Vector3(-0.02f, 0.1f, 0.0f);
                                    break;

                                case DecorationId.FieldstoneGate:
                                    amountToShift = 0.1f;
                                    amountToStretchFenceX = 0.0f;
                                    amountToStretchFenceY = -0.3f;
                                    amountToShiftMailboxX = 0.8f;
                                    amountToShiftMailboxY = 0.1f;
                                    amountToShiftMailboxZ = -0.2f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    shiftGate = new Vector3(0.0f, 0.0f, 0.0f);
                                    break;

                                case DecorationId.FarmGate:
                                    amountToShift = -4.5f;
                                    amountToStretchFenceX = -0.2f;
                                    amountToStretchFenceY = -0.2f;
                                    amountToShiftMailboxX = 1.6f;
                                    amountToShiftMailboxY = 0.0f;
                                    amountToShiftMailboxZ = -0.5f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    shiftGate = new Vector3(-0.5f, 0.0f, 0.0f);
                                    break;

                                case DecorationId.LatticeGate:
                                    amountToShift = -0.15f;
                                    amountToStretchFenceX = 0.0f;
                                    amountToStretchFenceY = 0.1f;
                                    amountToShiftMailboxX = 0.5f;
                                    amountToShiftMailboxY = 0.2f;
                                    amountToShiftMailboxZ = -0.2f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    shiftGate = new Vector3(0.0f, 0.0f, 0.0f);
                                    break;

                                case DecorationId.SimpleWoodenGate:
                                    amountToShift = 0.0f;
                                    amountToStretchFenceX = 0.0f;
                                    amountToStretchFenceY = 0.0f;
                                    amountToShiftMailboxX = 0.8f;
                                    amountToShiftMailboxY = 0.2f;
                                    amountToShiftMailboxZ = -0.2f;
                                    amountToStretchMailboxX = 0.0f;
                                    amountToStretchMailboxY = 0.0f;
                                    shiftGate = new Vector3(0.0f, 0.0f, 0.0f);
                                    break;
                            }

                            //change positions of fences
                            shiftVec = new Vector3(amountToShift, 0.0f, 0.0f);
                            leftFence.transform.position -= shiftVec;
                            rightFence.transform.position += shiftVec;
                            //change sizing of fences
                            sizeFence = new Vector3(amountToStretchFenceX, amountToStretchFenceY, 0.0f);
                            leftFence.transform.localScale += sizeFence;
                            rightFence.transform.localScale += sizeFence;
                            //change position of mailbox
                            mailboxShiftVec = new Vector3(amountToShiftMailboxX, amountToShiftMailboxY, amountToShiftMailboxZ);
                            currentMailbox.transform.position += mailboxShiftVec;
                            mailboxLogicObject.transform.position += mailboxShiftVec;
                            //change sizing of mailbox
                            mailboxStretchVec = new Vector3(amountToStretchMailboxX, amountToStretchMailboxY, 0.0f);
                            currentMailbox.transform.localScale += mailboxStretchVec;

                            gardenManager.UpdateDecorItem(areaIndex, currentMailbox.decorID, currentMailbox.isFlipped, currentMailbox.transform.position);
                            fenceShifted = true;

                            break;
                    }
                    AddFixtureItem(m_CurrentFixtureItem);
                    EventManager.instance.HandleEVENT_SUCCESSFUL_FIXTURE_PLACEMENT(m_CurrentFixtureItem.fixtureType);
                }
                else
                {
                    UpdateFixtureItem(m_CurrentFixtureItem);
                }

            }
            else
            {
                unsuccessfulPlacementSFX.Play();
                if (m_IsGeneratedFixtureItem)
                {
                    Destroy(m_CurrentFixtureItem.gameObject);
                    mailboxPlaceholder.SetActive(false);
                    gatePlaceholder.SetActive(false);
                    cottagePlaceholder.SetActive(false);
                }
                else
                {
                    m_CurrentFixtureItem.transform.position = m_OriginalPosition;
                    mailboxPlaceholder.SetActive(false);
                    gatePlaceholder.SetActive(false);
                    cottagePlaceholder.SetActive(false);
                }
            }

            m_IsGeneratedFixtureItem = false;
        }

        //TODO: adding a fixture means replacing the default asset with the new asset from inventory.
        //need to check if fixture matches the slot. if non default fixture is already there, swap and put the old one into inventory.
        public void AddFixtureItem(FixtureItem fixtureItem)
        {
            Vector3 fixturePos = fixtureItem.transform.position;
            switch (fixtureItem.fixtureType)
            {
                case FixtureType.Mailbox:
                    fixturePos = default_mailbox_pos;
                    fixturePos += mailboxShiftVec;
                    currentMailbox.transform.position = fixturePos;
                    mailboxLogicObject.transform.position = fixturePos;
                    //currentMailbox.transform
                    break;
                case FixtureType.Cottage:
                    fixturePos = defaultCottage.transform.position;
                    fixturePos += cottageShiftVec;
                    currentCottage.transform.position = fixturePos;
                    break;
                case FixtureType.Fence:
                    fixturePos = defaultGate.transform.position;
                    fixturePos -= shiftGate;
                    currentFence.transform.position = fixturePos;
                    break;
            }

            gardenManager.UpdateDecorItem(areaIndex, fixtureItem.decorID, fixtureItem.isFlipped, fixturePos);
            EventManager.instance.HandleEVENT_INVENTORY_REMOVE_DECOR((ushort)fixtureItem.decorID);

            fixtureItem.isPlaced = true;

            fixtureItem.onInteraction += StartFixtureCustomization;
            if (!fixtureItem.isDefault)
            {
                fixtureItem.GetComponentInChildren<DecorMenuUIDisplay>().onFlip += FlipDecorItem;
                fixtureItem.GetComponentInChildren<DecorMenuUIDisplay>().onUnFlip += UnFlipDecorItem;
                fixtureItem.GetComponentInChildren<DecorMenuUIDisplay>().onPutBack += RemoveFixtureItem;
            }
        }

        public void UpdateFixtureItem(FixtureItem fixtureItem)
        {
            if (!fixtureItem.isPlaced)
            {
                Vector3 fixturePos = fixtureItem.transform.position;
                switch (fixtureItem.fixtureType)
                {
                    case FixtureType.Mailbox:
                        fixturePos = defaultMailbox.transform.position;
                        fixturePos += mailboxShiftVec;
                        break;
                    case FixtureType.Cottage:
                        fixturePos = defaultCottage.transform.position;
                        break;
                    case FixtureType.Fence:
                        fixturePos = defaultGate.transform.position;
                        fixturePos -= shiftGate;
                        break;
                }
                fixtureItem.isPlaced = true;
                gardenManager.UpdateDecorItem(areaIndex, fixtureItem.decorID, fixtureItem.isFlipped, fixturePos, m_OriginalPosition);
            }
        }

        /// <summary>
        /// Removes fixture from fixture area and puts into inventory. Replaces with default fixture.
        /// </summary>
        /// <param name="fixtureItem"></param>
        public void RemoveFixtureItem(GardenDecorItem item)
        {
            FixtureItem fixtureItem = (FixtureItem)item;
            putInInventorySFX.Play();
            fixtureItem.isPlaced = false;
            cottagePlaceholder.SetActive(false);
            mailboxPlaceholder.SetActive(false);
            gatePlaceholder.SetActive(false);
            FixtureType t = fixtureItem.fixtureType;
            gardenManager.RemoveDecorItem(areaIndex, fixtureItem.decorID, fixtureItem.transform.position);
            EventManager.instance.HandleEVENT_INVENTORY_ADD_DECOR((ushort)fixtureItem.decorID);
            Destroy(fixtureItem.gameObject);
            FixtureItem f = null;
            switch (t)
            {
                case FixtureType.Mailbox:
                    f = defaultMailbox.GetComponent<FixtureItem>();
                    defaultMailbox.SetActive(true);
                    currentMailbox = f;
                    currMailboxID = (ushort)f.decorID;
                    break;
                case FixtureType.Cottage:
                    f = defaultCottage.GetComponent<FixtureItem>();
                    defaultCottage.SetActive(true);
                    currentCottage = f;
                    currCottageID = (ushort)f.decorID;
                    if(cottageShifted)
                    {
                        cottageShifted = false;
                        currentCottage.transform.position -= cottageShiftVec;
                        currentCottage.transform.localScale -= cottageStretchVec;
                    }
                    break;
                case FixtureType.Fence:
                    f = defaultGate.GetComponent<FixtureItem>();
                    defaultGate.SetActive(true);
                    currentFence = f;
                    currGateID = (ushort)f.decorID;
                    leftFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(currentFence);
                    rightFence.GetComponent<SpriteRenderer>().sprite = GetFenceSprite(currentFence);
                    if(fenceShifted)
                    {
                        fenceShifted = false;
                        leftFence.transform.position += shiftVec;
                        rightFence.transform.position -= shiftVec;
                        leftFence.transform.localScale -= sizeFence;
                        rightFence.transform.localScale -= sizeFence;
                        currentMailbox.transform.position -= mailboxShiftVec;
                        mailboxLogicObject.transform.position -= mailboxShiftVec;
                    }
                    break;
            }
            if (f)
            {
                UpdateFixtureItem(f);
            }
        }


        #endregion

        #region Cultivision

        public void ToggleCultivisionMode()
        {
            if (m_inCultivisionMode) ExitCultivisionMode();
            else EnterCultivisionMode();
        }

        public void EnterCultivisionMode()
        {
            m_inCultivisionMode = true;
            customizationCamera.gameObject.SetActive(true);
            playerController.entity.StopActions();
            playerController.entity.gameObject.SetActive(false);
            EventManager.instance.HandleEVENT_GOLEM_DISABLE();
            //make mailbox UI disabled
            mailboxLogic.SetActive(false);
            //GameObject.Find("Mailbox").GetComponent<MailboxUIDisplay>().inCustomization = true;
        }

        public void ExitCultivisionMode()
        {
            m_inCultivisionMode = false;
            customizationCamera.gameObject.SetActive(false);
            playerController.entity.gameObject.SetActive(true);
            EventManager.instance.HandleEVENT_GOLEM_ENABLE();

            //make mailbox UI enabled
            mailboxLogic.SetActive(true);
            //GameObject.Find("Mailbox").GetComponent<MailboxUIDisplay>().inCustomization = false;
        }

        #endregion
    }
}
