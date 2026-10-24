namespace NepalTrial.Trial
{
    /// <summary>Unity's positive Y heading is a clockwise/right turn.</summary>
    public static class TwoWheelerIndicatorRules
    {
        public static bool HasCorrectTurnSignal(float signedTurnDegrees,bool signaledLeft,bool signaledRight)
        {
            return signedTurnDegrees > 0f ? signaledRight :
                signedTurnDegrees < 0f && signaledLeft;
        }
    }
}
