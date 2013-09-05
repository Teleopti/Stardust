namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// What section of the budget table that a budget row belongs to
    /// </summary>
    public enum BudgetSection
    {
        /// <summary>
        /// First section
        /// </summary>
        FirstSystem,
        /// <summary>
        /// the section containing shrinkages
        /// </summary>
        Shrinkage,
        /// <summary>
        /// The section after shrinkages
        /// </summary>
        SecondSystem,
        /// <summary>
        /// The section containing (in)efficiencies
        /// </summary>
        Efficiency,
        /// <summary>
        /// The last section
        /// </summary>
        LastSystem,
    }
}