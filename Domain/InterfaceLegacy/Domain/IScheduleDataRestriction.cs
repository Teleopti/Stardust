
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IScheduleDataRestriction: IScheduleData
    {
        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        DateOnly RestrictionDate { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is availability restriction.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is availability restriction; otherwise, <c>false</c>.
        /// </value>
        bool IsAvailabilityRestriction { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is rotation restriction.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is rotation restriction; otherwise, <c>false</c>.
        /// </value>
        bool IsRotationRestriction { get; }
        /// <summary>
        /// Gets or sets the restriction.
        /// </summary>
        /// <value>The restriction.</value>
        IRestrictionBase Restriction { get;  }

        /// <summary>
        /// Gets a value indicating whether this instance is preference restriction.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is preference restriction; otherwise, <c>false</c>.
        /// </value>
        bool IsPreferenceRestriction { get; }
    }
}
