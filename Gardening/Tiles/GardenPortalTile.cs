using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Core.SceneManagement;
using GrandmaGreen.Entities;
using Cinemachine;
using Core.Utilities;
using DG.Tweening;
using GrandmaGreen.Garden;

namespace GrandmaGreen
{
    [CreateAssetMenu(menuName = "GrandmaGreen/Tiles/GardenPortalTile", fileName = "GardenPortalTile")]
    public class GardenPortalTile : Tile, IGameTile
    {
        public Direction portalDirection;
        public AreaExitState exitState;
        public AreaServices areaServicer;
        public GardenTransitionData gardenTransitionData;
        public GardenIndexTracker indexTracker;
        public int destinationIndex;
        public GardenManager gardenManager;

        public void DoTileAction(EntityController entity)
        {
            exitState.exitSide = portalDirection;
            GardenCameraTransition(entity).Start();
        }

        public IEnumerator GardenCameraTransition(EntityController entity)
        {
            entity.PauseController();

            CinemachineVirtualCamera playerCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;

            Direction exitDirection = portalDirection + 2;
            if ((int)exitDirection > 3)
                exitDirection -= 4;

            entity.SetDestination(
                gardenTransitionData
                .transitions[destinationIndex]
                .enterancePoints[(int)exitDirection].position, false);
            
            gardenTransitionData.transitions[destinationIndex].camera.gameObject.SetActive(true);
            gardenTransitionData.transitions[destinationIndex].camera.m_Follow =  playerCam.m_Follow;
            playerCam.m_Follow = null;
            playerCam.gameObject.SetActive(false);

            indexTracker.currentGardenIndex = destinationIndex;
            //gardenManager.RegisterGarden(destinationIndex);
            EventManager.instance.HandleEVENT_CHANGE_GARDEN_INDEX(destinationIndex);


            yield return new WaitForSeconds(2.5f);

            entity.StartController();
        }
    }
}
