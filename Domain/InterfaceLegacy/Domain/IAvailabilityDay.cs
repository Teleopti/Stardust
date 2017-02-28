
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAvailabilityDay
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        int Index { get; }

        /// <summary>
        /// Gets or sets the restriction.
        /// </summary>
        /// <value>The restriction.</value>
        IAvailabilityRestriction Restriction { get; set; }
    }
}
