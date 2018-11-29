using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Used for extracting OpenAbsenceRequestPeriods that matches current ViewPointDate
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-15
    /// </remarks>
    public interface IOpenAbsenceRequestPeriodExtractor
    {
        /// <summary>
        /// Gets or sets the view point date.
        /// </summary>
        /// <value>The view point date.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-15
        /// </remarks>
        DateOnly ViewpointDate { get; set; }
        /// <summary>
        /// Gets the available periods.
        /// </summary>
        /// <value>The available periods.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-15
        /// </remarks>
        IEnumerable<IAbsenceRequestOpenPeriod> AvailablePeriods { get; }

        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <value>The projection.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-04-22
        /// </remarks>
        IOpenAbsenceRequestPeriodProjection Projection { get; }

		/// <summary>
		/// Gets all periods.		
		/// </summary>
	    IEnumerable<IAbsenceRequestOpenPeriod> AllPeriods { get; }
    }
}