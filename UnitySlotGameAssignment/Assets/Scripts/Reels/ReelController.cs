using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotGame.Data;
using SlotGame.Audio;

namespace SlotGame.Reels
{
    /// <summary>
    /// Controls smooth vertical symbol scrolling, wrapping, staggered landing, and win highlight feedback for a single reel.
    /// </summary>
    public class ReelController : MonoBehaviour
    {
        [Header("Reel Slots")]
        [Tooltip("Symbol slot RectTransforms ordered top to bottom (include top/bottom buffer slots if masked).")]
        [SerializeField] private List<RectTransform> symbolSlots = new List<RectTransform>();

        [Tooltip("Index of the slot on the active center evaluation line.")]
        [SerializeField] private int centerSlotIndex = 1;

        [SerializeField] private float slotSpacing = 140f;
        [SerializeField] private float spinSpeed = 1600f;

        [Header("Animation Feel")]
        [SerializeField] private float settleDuration = 0.14f;
        [SerializeField] private float winPulseScale = 1.12f;
        [SerializeField] private float winPulseDuration = 0.35f;

        private readonly List<Image> _slotImages = new List<Image>();
        private IReadOnlyList<SymbolDefinition> _availableSymbols;
        private SlotRNG _rng;
        private bool _isSpinning;

        public SymbolDefinition CurrentCenterSymbol { get; private set; }

        private void Awake()
        {
            _slotImages.Clear();
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                RectTransform slot = symbolSlots[i];
                _slotImages.Add(slot != null ? slot.GetComponent<Image>() : null);
            }
        }

        public void Initialize(IReadOnlyList<SymbolDefinition> symbols, SlotRNG rng)
        {
            _availableSymbols = symbols;
            _rng = rng;
            RandomizeInitialBoard();
        }

        public void RandomizeInitialBoard()
        {
            if (_availableSymbols == null || _availableSymbols.Count == 0 || _rng == null) 
                return;

            for (int i = 0; i < _slotImages.Count; i++)
            {
                SymbolDefinition picked = _rng.PickWeightedSymbol(_availableSymbols);
                SetSlotSymbol(i, picked);
            }

            if (centerSlotIndex >= 0 && centerSlotIndex < _slotImages.Count)
                CurrentCenterSymbol = GetSymbolFromImage(_slotImages[centerSlotIndex]);
        }

        public IEnumerator SpinUntilTarget(float minSpinDuration, SymbolDefinition targetSymbol, Action onReelStopped = null)
        {
            if (_isSpinning || symbolSlots.Count == 0) yield break;
            _isSpinning = true;

            float elapsed = 0f;
            float wrapBoundaryY = -slotSpacing * ((symbolSlots.Count - 1) * 0.5f + 1f);

            while (elapsed < minSpinDuration)
            {
                float deltaMove = spinSpeed * Time.deltaTime;
                MoveSlotsDown(deltaMove, wrapBoundaryY);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (centerSlotIndex >= 0 && centerSlotIndex < _slotImages.Count && targetSymbol != null)
            {
                SetSlotSymbol(centerSlotIndex, targetSymbol);
                CurrentCenterSymbol = targetSymbol;
            }

            yield return StartCoroutine(SettleSnapRoutine());

            if (SlotAudioManager.Instance != null)
                SlotAudioManager.Instance.PlayReelStop();
            
            _isSpinning = false;
            onReelStopped?.Invoke();
        }

        public IEnumerator PlayWinHighlightRoutine()
        {
            if (centerSlotIndex < 0 || centerSlotIndex >= symbolSlots.Count) 
                yield break;

            RectTransform targetSlot = symbolSlots[centerSlotIndex];

            if (targetSlot == null) 
                yield break;

            Vector3 baseScale = Vector3.one;
            Vector3 peakScale = Vector3.one * winPulseScale;
            float halfDuration = Mathf.Max(0.05f, winPulseDuration * 0.5f);

            float t = 0f;
            while (t < halfDuration)
            {
                t += Time.deltaTime;
                targetSlot.localScale = Vector3.Lerp(baseScale, peakScale, Mathf.Clamp01(t / halfDuration));
                yield return null;
            }

            t = 0f;
            while (t < halfDuration)
            {
                t += Time.deltaTime;
                targetSlot.localScale = Vector3.Lerp(peakScale, baseScale, Mathf.Clamp01(t / halfDuration));
                yield return null;
            }

            targetSlot.localScale = baseScale;
        }

        private void MoveSlotsDown(float deltaMove, float wrapBoundaryY)
        {
            float fullCycleHeight = slotSpacing * symbolSlots.Count;

            for (int i = 0; i < symbolSlots.Count; i++)
            {
                RectTransform slot = symbolSlots[i];
                if (slot == null) 
                    continue;

                Vector2 pos = slot.anchoredPosition;
                pos.y -= deltaMove;

                if (pos.y < wrapBoundaryY)
                {
                    pos.y += fullCycleHeight;
                    if (_availableSymbols != null && _availableSymbols.Count > 0 && _rng != null)
                        SetSlotSymbol(i, _rng.PickWeightedSymbol(_availableSymbols));
                }

                slot.anchoredPosition = pos;
            }
        }

        private IEnumerator SettleSnapRoutine()
        {
            Vector2[] startPositions = new Vector2[symbolSlots.Count];
            Vector2[] targetPositions = new Vector2[symbolSlots.Count];

            float halfSpan = (symbolSlots.Count - 1) * 0.5f;
            for (int i = 0; i < symbolSlots.Count; i++)
            {
                RectTransform slot = symbolSlots[i];
                startPositions[i] = slot != null ? slot.anchoredPosition : Vector2.zero;
                float targetY = (halfSpan - i) * slotSpacing;
                targetPositions[i] = new Vector2(startPositions[i].x, targetY);
            }

            float t = 0f;
            while (t < settleDuration)
            {
                t += Time.deltaTime;
                float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / settleDuration), 3f);

                for (int i = 0; i < symbolSlots.Count; i++)
                    if (symbolSlots[i] != null)
                        symbolSlots[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], eased);
                
                yield return null;
            }

            for (int i = 0; i < symbolSlots.Count; i++)
            {
                if (symbolSlots[i] != null)
                    symbolSlots[i].anchoredPosition = targetPositions[i];
            }
        }

        private void SetSlotSymbol(int index, SymbolDefinition symbol)
        {
            if (index < 0 || index >= _slotImages.Count || _slotImages[index] == null || symbol == null) 
                return;

            _slotImages[index].sprite = symbol.icon;
            _slotImages[index].name = $"Symbol_{symbol.symbolId}";
        }

        private SymbolDefinition GetSymbolFromImage(Image img)
        {
            if (img == null || _availableSymbols == null) 
                return null;

            for (int i = 0; i < _availableSymbols.Count; i++)
            {
                SymbolDefinition sym = _availableSymbols[i];

                if (sym != null && sym.icon == img.sprite)
                    return sym;
            }
            return null;
        }
    }
}
