namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// A custom efficiency shrinkage item
    ///</summary>
    public interface ICustomEfficiencyShrinkage : IAggregateEntity
    {
        ///<summary>
        /// Gets or sets the name of this custom efficiency shrinkage item
        ///</summary>
        string ShrinkageName { get; set; }

        ///<summary>
        /// Efficiency shrinkage included in request allowance
        ///</summary>
        bool IncludedInAllowance { get; set; }
    }
}