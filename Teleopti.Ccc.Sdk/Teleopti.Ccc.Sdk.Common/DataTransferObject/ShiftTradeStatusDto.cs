namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Status modes for Shift Trades
    /// </summary>
    public enum ShiftTradeStatusDto
    {
        /// <summary>
        /// Ok by initiating person
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