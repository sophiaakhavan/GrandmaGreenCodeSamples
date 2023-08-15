using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Input;

namespace GrandmaGreen.Garden
{
    public class GardenDecorItem : MonoBehaviour
    {
        public BoxCollider interactable;
        public BoxCollider boundsCollider;
        public SpriteRenderer sprite;
        public Collections.DecorationId decorID;
        [SerializeField] GardenCustomizer customizer;
        public bool isPlaced;
        public bool isFlipped;

        public System.Action<GardenDecorItem> onInteraction;
        
        public void EnableInteraction() => interactable.enabled = true;
        public void DisableInteraction() => interactable.enabled = false;
        public void ToggleInteraction() => interactable.enabled = !interactable.enabled;

        public DecorMenuUIDisplay decorMenu;
        public float tapTimeBound = 0.2f;
        private float m_tapTime;
        private bool m_tapStart = false;
        private Coroutine m_dragCoroutine;

        void OnEnable()
        {
            EventManager.instance.EVENT_TOGGLE_CUSTOMIZATION_MODE += ToggleInteraction;
        }

        void OnDisable()
        {
            EventManager.instance.EVENT_TOGGLE_CUSTOMIZATION_MODE -= ToggleInteraction;
        }

        public void OnDecorTapped()
        {
            m_tapTime = Time.time;
            m_tapStart = true;

            m_dragCoroutine = StartCoroutine(DecorDragHandler());
        }

        IEnumerator DecorDragHandler()
        {
            float time = 0.0f;
            while (time < tapTimeBound)
            {
                time = Time.time - m_tapTime;
                yield return null;
            }
            decorMenu.CloseUI();
            onInteraction?.Invoke(this);
        }

        public void OnDecorTapEnd()
        {
            float tappingTime = Time.time - m_tapTime;

            // If it's a light tap within the tap bound, open the UI.
            if (tappingTime > 0 && tappingTime < tapTimeBound)
            {
                StopCoroutine(m_dragCoroutine);
                if (decorMenu.displayOpen)
                {
                    decorMenu.CloseUI();
                }
                else
                {
                    decorMenu.OpenUI();
                }
            }
            m_tapStart = false;
        }
    }
}
