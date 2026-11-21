using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotGame.Data;
using SlotGame.Reels;
using SlotGame.Audio;

namespace SlotGame.Core
{
    /// <summary>
    /// Orchestrates balance, bet tiers, staggered reel spins, all-match payout evaluation, and win feedback.
    /// </summary>
    public class SlotGameManager : MonoBehaviour
    {
        [Header("Economy & Bets")]
        [SerializeField] private float startingBalance = 500f;
        [SerializeField] private List<float> availableBets = new List<float> { 10f, 50f, 100f };
        [SerializeField] private int defaultBetIndex = 0;

        [Header("Symbols, Reels & Payouts")]
        [SerializeField] private List<SymbolDefinition> symbols = new List<SymbolDefinition>();
        [SerializeField] private List<ReelController> reels = new List<ReelController>();
        [SerializeField] private PayoutConfig payoutConfig;
        [SerializeField] private bool isBonusActive = false;

        [Header("Win Rate Control")]
        [Tooltip("Target win probability per spin (0 = never win, 1 = always all-match win).")]
        [Range(0f, 1f)]
        public float winChance = 0.35f;

        [Header("Spin Timing")]
        [SerializeField] private float baseSpinDuration = 0.8f;
        [SerializeField] private float reelStopStagger = 0.35f, settleBufferSeconds = 0.22f, winHoldDuration = 1.2f;

        private SlotRNG _rng;
        private readonly List<SymbolDefinition> _landedSymbols = new List<SymbolDefinition>();

        public GameState CurrentState = GameState.Idle;
        public float Balance;
        public float CurrentBet;
        public float LastWin;
        public SymbolDefinition LastWinningSymbol;

        private void Awake()
        {
            _rng = new SlotRNG();
            Balance = startingBalance;

            int safeIdx = Mathf.Clamp(defaultBetIndex, 0, Mathf.Max(0, availableBets.Count - 1));
            CurrentBet = availableBets.Count > 0 ? availableBets[safeIdx] : 10f;

            for (int i = 0; i < reels.Count; i++)
                if (reels[i] != null)
                    reels[i].Initialize(symbols, _rng);
        }

        public void SelectBet(float betAmount)
        {
            if (CurrentState != GameState.Idle)
                return;

            if (betAmount <= 0f)
                return;

            CurrentBet = betAmount;
        }

        public bool CanSpin()
        {
            if (CurrentState != GameState.Idle)
                return false;

            if (symbols == null || symbols.Count == 0 || reels == null || reels.Count == 0)
            {
                Debug.LogWarning("SlotGameManager requires at least one symbol and one reel configured.");
                return false;
            }

            if (Balance < CurrentBet)
            {
                Debug.LogWarning("Insufficient balance to place current bet.");
                return false;
            }

            return true;
        }

        public IEnumerator SpinSequenceRoutine()
        {
            CurrentState = GameState.Spinning;
            LastWin = 0f;
            LastWinningSymbol = null;
            Balance -= CurrentBet;

            if (SlotAudioManager.Instance != null)
                SlotAudioManager.Instance.PlaySpinStart();
            
            _landedSymbols.Clear();
            GenerateSpinOutcome(_landedSymbols);

            for (int i = 0; i < reels.Count; i++)
            {
                SymbolDefinition targetSymbol = i < _landedSymbols.Count
                    ? _landedSymbols[i]
                    : _rng.PickWeightedSymbol(symbols);

                if (reels[i] != null)
                {
                    float durationForReel = baseSpinDuration + (i * reelStopStagger);
                    StartCoroutine(reels[i].SpinUntilTarget(durationForReel, targetSymbol));
                }
            }

            float totalWaitTime = baseSpinDuration + (Mathf.Max(0, reels.Count - 1) * reelStopStagger) + settleBufferSeconds;
            yield return new WaitForSeconds(totalWaitTime);

            CurrentState = GameState.Evaluating;
            SpinEvaluationResult result = SlotEvaluator.EvaluateAllMatch(
                _landedSymbols,
                CurrentBet,
                payoutConfig,
                isBonusActive);

            if (result.IsWin)
            {
                CurrentState = GameState.WinCelebration;
                LastWin = result.Payout;
                LastWinningSymbol = result.WinningSymbol;
                Balance += LastWin;

                if (SlotAudioManager.Instance != null)
                    SlotAudioManager.Instance.PlayWin();

                for (int i = 0; i < reels.Count; i++)
                    if (reels[i] != null)
                        StartCoroutine(reels[i].PlayWinHighlightRoutine());

                yield return new WaitForSeconds(winHoldDuration);
            }
            else
            {
                if (SlotAudioManager.Instance != null)
                    SlotAudioManager.Instance.PlayLose();
            }

            CurrentState = GameState.Idle;
        }

        private void GenerateSpinOutcome(List<SymbolDefinition> outputSymbols)
        {
            if (reels.Count == 0 || symbols.Count == 0) return;

            bool forceWin = UnityEngine.Random.value < winChance;

            if (forceWin)
            {
                SymbolDefinition sharedSymbol = _rng.PickWeightedSymbol(symbols);
                for (int i = 0; i < reels.Count; i++)
                {
                    outputSymbols.Add(sharedSymbol);
                }
                return;
            }

            for (int i = 0; i < reels.Count; i++)
            {
                outputSymbols.Add(_rng.PickWeightedSymbol(symbols));
            }

            if (symbols.Count > 1 && AllSymbolsIdentical(outputSymbols))
            {
                SymbolDefinition first = outputSymbols[0];
                for (int i = 0; i < symbols.Count; i++)
                {
                    if (symbols[i] != null && symbols[i] != first)
                    {
                        outputSymbols[outputSymbols.Count - 1] = symbols[i];
                        break;
                    }
                }
            }
        }

        private bool AllSymbolsIdentical(IReadOnlyList<SymbolDefinition> list)
        {
            if (list.Count <= 1) return true;
            SymbolDefinition first = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] != first) return false;
            }
            return true;
        }

        public void ResetBalanceAndWin()
        {
            if (CurrentState != GameState.Idle) 
                return;

            Balance = startingBalance;
            LastWin = 0f;
            LastWinningSymbol = null;

            int safeIdx = Mathf.Clamp(defaultBetIndex, 0, Mathf.Max(0, availableBets.Count - 1));
            CurrentBet = availableBets.Count > 0 ? availableBets[safeIdx] : 10f;
        }

    }
}
