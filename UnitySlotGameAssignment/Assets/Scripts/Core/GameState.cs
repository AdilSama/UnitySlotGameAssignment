namespace SlotGame.Core
{
    /// <summary>
    /// Shared lifecycle states for the slot machine loop.
    /// </summary>
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        WinCelebration
    }
}
