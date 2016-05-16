using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsAbsenceRepository
	{
		IList<IAnalyticsAbsence> Absences();
	}
}