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
		public List<SkillForecast> SkillForecasts = new List<SkillForecast>();

		public void PersistSkillForecast(List<SkillForecast> listOfIntervals)
		{
			SkillForecasts.AddRange(listOfIntervals);
		}

		public IList<SkillForecast> LoadSkillForecast(Guid[] skills, DateTimePeriod period)
		{
			var skillForecastList = SkillForecasts.Where(x =>x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && x.IsBackOffice == false).ToList();
			skillForecastList.AddRange(SkillForecasts.Where(x =>
				x.StartDateTime >= period.StartDateTime.AddDays(-8) && x.EndDateTime <= period.EndDateTime &&
				x.IsBackOffice == true).ToList());

			return skillForecastList;
		}
	}
}
