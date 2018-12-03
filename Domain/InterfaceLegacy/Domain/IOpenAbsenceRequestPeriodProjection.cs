using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Creates projection of multiple intersecting IOpenAbsenceRequestPeriod's
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-16
    /// </remarks>
    public interface IOpenAbsenceRequestPeriodProjection
    {
		/// <summary>
		/// Gets the projected periods.
		/// </summary>
		/// <param name="limitToPeriod">The limit to date only period.</param>
		/// <param name="date">Date culture of the person sending the request.</param>
		/// <param name="language">Language culture of the person sending the request.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2010-04-16
		/// </remarks>
		IList<IAbsenceRequestOpenPeriod> GetProjectedPeriods(DateOnlyPeriod limitToPeriod, CultureInfo date, CultureInfo language);
    }
}