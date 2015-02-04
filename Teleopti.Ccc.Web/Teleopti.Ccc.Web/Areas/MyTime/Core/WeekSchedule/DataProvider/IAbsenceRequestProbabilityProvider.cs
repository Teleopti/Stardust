using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IAbsenceRequestProbabilityProvider
	{
		List<IAbsenceRequestProbability> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period);
	}
}