using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceRequestProbabilityProvider
	{
		List<IAbsenceRequestProbability> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period);
	}
}