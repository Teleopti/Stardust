using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsAbsenceRepository
	{
		IList<AnalyticsAbsence> Absences();
	}
}