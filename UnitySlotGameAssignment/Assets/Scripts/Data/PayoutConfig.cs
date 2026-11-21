using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame.Data
{
    [Serializable]
    public struct SymbolPayoutOverride
    {
        [Tooltip("Symbol that receives a custom payout multiplier.")]
        public SymbolDefinition symbol;

        [Tooltip("Custom multiplier when all slots match this symbol. Set <= 0 to fall back to symbol/default multiplier.")]
        [Min(0f)]
        public float overrideMultiplier;
    }

    [CreateAssetMenu(fileName = "PayoutConfig", menuName = "Slot Game/Payout Config")]
    public class PayoutConfig : ScriptableObject
    {
        [Header("Default Payout Rules")]
        [Tooltip("Fallback multiplier when all slots match and no symbol/override multiplier is set.")]
        [Min(0f)]
        public float defaultAllMatchMultiplier = 5f;

        [Tooltip("Multiplier applied on top of base payout when bonus mode is active.")]
        [Min(1f)]
        public float bonusMultiplier = 1f;

        [Header("Symbol Overrides")]
        [SerializeField] private List<SymbolPayoutOverride> symbolOverrides = new List<SymbolPayoutOverride>();

        /// <summary>
        /// Computes the final multiplier for an all-match win on the given symbol.
        /// </summary>
        public float GetMultiplier(SymbolDefinition symbol, bool isBonusActive = false)
        {
            float multiplier = defaultAllMatchMultiplier;

            if (symbol != null)
            {
                if (symbol.allMatchMultiplier > 0f)
                {
                    multiplier = symbol.allMatchMultiplier;
                }

                for (int i = 0; i < symbolOverrides.Count; i++)
                {
                    SymbolPayoutOverride entry = symbolOverrides[i];
                    if (entry.symbol == symbol && entry.overrideMultiplier > 0f)
                    {
                        multiplier = entry.overrideMultiplier;
                        break;
                    }
                }
            }

            if (isBonusActive && bonusMultiplier > 0f)
            {
                multiplier *= bonusMultiplier;
            }

            return multiplier;
        }
    }
}
