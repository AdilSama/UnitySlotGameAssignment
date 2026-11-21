using UnityEngine;

namespace SlotGame.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "Slot Game/Symbol Definition")]
    public class SymbolDefinition : ScriptableObject
    {
        [Tooltip("Unique identifier used for evaluation, UI, and debugging.")]
        public string symbolId;

        [Tooltip("Visual sprite rendered inside reel symbol slots.")]
        public Sprite icon;

        [Tooltip("Higher weight increases the probability of selection in SlotRNG.")]
        [Min(1)]
        public int weight = 10;

        [Tooltip("Payout multiplier applied to the current bet when all slots match this symbol.")]
        [Min(0f)]
        public float allMatchMultiplier = 5f;
    }
}
