using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public IList<SkillForecast> LoadSkillForecast(Guid[] skills, DateTimePeriod period)
		{
			return SkillForecasts.Where(x =>x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime).ToList();
		}
	}
}
