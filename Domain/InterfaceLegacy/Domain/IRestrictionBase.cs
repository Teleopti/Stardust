
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRestrictionBase : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the start time limitation.
        /// </summary>
        /// <value>The start time limitation.</value>
        StartTimeLimitation StartTimeLimitation { get; set; }
        /// <summary>
        /// Gets or sets the end time limitation.
        /// </summary>
        /// <value>The end time limitation.</value>
        EndTimeLimitation EndTimeLimitation { get; set; }
        /// <summary>
        /// Gets or sets the work time limitation.
        /// </summary>
        /// <value>The work time limitation.</value>
        WorkTimeLimitation WorkTimeLimitation { get; set; }

        /// <summary>
        /// Determines whether this instance is restriction.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is restriction; otherwise, <c>false</c>.
        /// </returns>
        bool IsRestriction();
    }
}
