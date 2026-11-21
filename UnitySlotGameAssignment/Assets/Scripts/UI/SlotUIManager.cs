using SlotGame.Core;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.UI
{
    /// <summary>
    /// Binds player HUD readouts and spin/bet/exit controls to SlotGameManager.
    /// </summary>
    public class SlotUIManager : MonoBehaviour
    {
        [Header("Core Reference")]
        [SerializeField] private SlotGameManager gameManager;

        [Header("HUD Texts")]
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text betText, winText, statusText;

        [Header("Controls")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private List<Button> betButtons;
        [SerializeField] private Slider WinChanceSlider;
        [SerializeField] private TMP_Text winChanceTxt;

        [Header("Formatting")]
        [SerializeField] private string balancePrefix = "Balance: ";
        [SerializeField] private string betPrefix = "Bet: ", winPrefix = "Win: ";

        [Header("Pin")]
        [SerializeField] GameObject PinFull;
        [SerializeField] GameObject PinHalf;

        private void OnEnable() => RefreshUI();

        private void Update() => RefreshUI();

        public void OnSpinClicked()
        {
            if (gameManager == null)
                return;
            if (gameManager.CanSpin())
            {
                StartCoroutine(gameManager.SpinSequenceRoutine());
                StartCoroutine(PinRoutine());
            }
            RefreshUI();
        }

        public void OnBetSelected(float amount)
        {
            if (gameManager == null) 
                return;

            gameManager.SelectBet(amount);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (gameManager == null)
                return;

            bool isIdle = gameManager.CurrentState == GameState.Idle;

            balanceText.text = $"{balancePrefix}{gameManager.Balance:0.##}";
            betText.text = $"{betPrefix}{gameManager.CurrentBet:0.##}";
            winText.text = $"{winPrefix}{gameManager.LastWin:0.##}";

            statusText.text = GetStatusMessage(gameManager.CurrentState);
            spinButton.interactable = isIdle && gameManager.Balance >= gameManager.CurrentBet;

            betButtons.ForEach(btn => btn.interactable = isIdle);
            resetButton.interactable = isIdle;

            float val = WinChanceSlider.value / 100f;
            gameManager.winChance = val;
            winChanceTxt.text = val.ToString();
        }

        private string GetStatusMessage(GameState state)
        {
            return state switch
            {
                GameState.Spinning => "Spinning...",
                GameState.Evaluating => "Evaluating...",
                GameState.WinCelebration => gameManager.LastWinningSymbol != null
                                        ? $"All Match ({gameManager.LastWinningSymbol.symbolId})!"
                                        : "Jackpot Win!",
                _ => gameManager.Balance < gameManager.CurrentBet ? "Insufficient Balance" : "Ready",
            };
        }

        public void OnResetClicked()
        {
            if (gameManager == null) return;
            gameManager.ResetBalanceAndWin();
            RefreshUI();
        }

        IEnumerator PinRoutine()
        { 
            PinFull.SetActive(false);
            PinHalf.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            PinFull.SetActive(true);
            PinHalf.SetActive(false);
        }

    }
}
