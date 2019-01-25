using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISkillForecastReadModelRepository

	{
		void PersistSkillForecast(List<SkillForecast> listOfIntervals);
		IList<SkillForecast> LoadSkillForecast(Guid[] skills, DateTime startDateTime, DateTime endDateTime);
	}
}