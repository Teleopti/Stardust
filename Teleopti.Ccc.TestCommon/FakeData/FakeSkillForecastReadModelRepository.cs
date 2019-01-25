using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillForecastReadModelRepository : ISkillForecastReadModelRepository
	{
		public IList<SkillForecast> SkillForecasts;

		public void PersistSkillForecast(List<SkillForecast> listOfIntervals)
		{
			throw new NotImplementedException();
		}

		public IList<SkillForecast> LoadSkillForecast(Guid[] skills, DateTime startDateTime, DateTime endDateTime)
		{
			return SkillForecasts;
		}
	}
}
