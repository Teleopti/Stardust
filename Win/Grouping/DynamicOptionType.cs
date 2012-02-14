namespace Teleopti.Ccc.Win.Grouping
{
    /// <summary>
    /// enumerater for  GroupPage
    /// </summary>
    public enum DynamicOptionType
    {
        /// <summary>
        /// Default group
        /// </summary>
        DoNotGroup,

        /// <summary>
        /// Person.Note
        /// </summary>
        PersonNote,

        /// <summary>
        /// PersonPeriod.Contract
        /// </summary>
        Contract,

        /// <summary>
        /// PersonPeriod.PartTimePercentage
        /// </summary>
        PartTimePercentage,

        /// <summary>
        /// PersonPeriod.ContractSchedule
        /// </summary>
        ContractSchedule,

        /// <summary>
        /// PersonPeriod.RulesetBag
        /// </summary>
        RuleSetBag,

        /// <summary>
        /// PersonPeriod.OptionalPage
        /// </summary>
        OptionalPage,

        /// <summary>
        /// BusinessHierarchy structure.
        /// </summary>
        BusinessHierarchy
    }
}