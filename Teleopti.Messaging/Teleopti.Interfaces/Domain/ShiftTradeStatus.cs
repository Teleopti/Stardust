namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Status modes for Shift Trades
    /// </summary>
    public enum ShiftTradeStatus
    {
        /// <summary>
        /// Ok ny initiating person
        /// </summary>
        OkByMe,
        /// <summary>
        /// Ok by both parts
        /// </summary>
        OkByBothParts,
        /// <summary>
        /// Shift Trade not valid due to changes in schedule
        /// </summary>
        NotValid,
        /// <summary>
        /// Conditions has changed since the request was created, hence status is Referred.
        /// </summary>
        Referred
    }
}
