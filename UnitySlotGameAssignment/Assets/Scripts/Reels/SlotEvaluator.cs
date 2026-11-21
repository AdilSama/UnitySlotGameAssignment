using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Reels
{
    public struct SpinEvaluationResult
    {
        public bool IsWin;
        public SymbolDefinition WinningSymbol;
        public float Payout;
    }

    /// <summary>
    /// Evaluates slot outcomes under the assignment rule: player wins when all slots show the same symbol.
    /// </summary>
    public static class SlotEvaluator
    {
        public static SpinEvaluationResult EvaluateAllMatch(
            IReadOnlyList<SymbolDefinition> landedSymbols,
            float betAmount,
            PayoutConfig payoutConfig = null,
            bool isBonusActive = false)
        {
            if (landedSymbols == null || landedSymbols.Count == 0 || betAmount <= 0f)
            {
                return new SpinEvaluationResult { IsWin = false, WinningSymbol = null, Payout = 0f };
            }

            SymbolDefinition firstSymbol = landedSymbols[0];
            if (firstSymbol == null)
            {
                return new SpinEvaluationResult { IsWin = false, WinningSymbol = null, Payout = 0f };
            }

            for (int i = 1; i < landedSymbols.Count; i++)
            {
                if (landedSymbols[i] != firstSymbol)
                {
                    return new SpinEvaluationResult { IsWin = false, WinningSymbol = null, Payout = 0f };
                }
            }

            float multiplier = payoutConfig != null
                ? payoutConfig.GetMultiplier(firstSymbol, isBonusActive)
                : (firstSymbol.allMatchMultiplier > 0f ? firstSymbol.allMatchMultiplier : 5f);

            float payout = betAmount * multiplier;
            return new SpinEvaluationResult
            {
                IsWin = true,
                WinningSymbol = firstSymbol,
                Payout = payout
            };
        }
    }
}
