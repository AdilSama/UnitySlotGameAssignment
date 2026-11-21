using System;
using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Reels
{
    /// <summary>
    /// Weighted random selector for slot symbols.
    /// </summary>
    public class SlotRNG
    {
        private readonly Random _rng;

        public SlotRNG(int? seed = null)
        {
            // Uses a time-based seed by default; pass an explicit seed for reproducible tests.
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public SymbolDefinition PickWeightedSymbol(IReadOnlyList<SymbolDefinition> symbols)
        {
            if (symbols == null || symbols.Count == 0)
                throw new ArgumentException("Symbol list cannot be null or empty.", nameof(symbols));

            int totalWeight = 0;
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolDefinition current = symbols[i];
                if (current != null && current.weight > 0)
                {
                    totalWeight += current.weight;
                }
            }

            if (totalWeight <= 0)
                throw new InvalidOperationException("Total symbol weight must be greater than zero.");

            int roll = _rng.Next(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolDefinition current = symbols[i];
                if (current == null || current.weight <= 0) continue;

                cumulative += current.weight;
                if (roll < cumulative)
                {
                    return current;
                }
            }

            for (int i = symbols.Count - 1; i >= 0; i--)
            {
                if (symbols[i] != null) return symbols[i];
            }

            throw new InvalidOperationException("No valid SymbolDefinition found in list.");
        }
    }
}
