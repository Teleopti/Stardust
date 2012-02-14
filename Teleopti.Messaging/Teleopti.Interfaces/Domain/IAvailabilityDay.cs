
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

        /// <summary>
        /// Determines whether [is availability day].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is availability day]; otherwise, <c>false</c>.
        /// </returns>
        bool IsAvailabilityDay();
    }
}
