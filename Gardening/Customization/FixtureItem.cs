using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Input;
using GrandmaGreen.Collections;

namespace GrandmaGreen.Garden
{
    public class FixtureItem : GardenDecorItem
    {
        [SerializeField] FixtureCustomizer fcustomizer;

        public new System.Action<FixtureItem> onInteraction;

        public new void SendInteractionAction() => onInteraction?.Invoke(this);

        public bool isDefault;

        public FixtureType fixtureType;
    }
}
