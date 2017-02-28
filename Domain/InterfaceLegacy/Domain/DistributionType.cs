namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// To determine how distribution will be made
    /// </summary>
    public enum DistributionType
    {
        /// <summary>
        /// Adds or subtracts the same amount for each item
        /// </summary>
        Even,
        /// <summary>
        /// Adds or subtracts based on the item's current size compared to total size
        /// </summary>
        ByPercent
    }
}