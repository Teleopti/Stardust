using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class MissingForecastProvider : IMissingForecastProvider
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IExistingForecastRepository _existingForecastRepository;

		public MissingForecastProvider(IScenarioRepository scenarioRepository, IExistingForecastRepository existingForecastRepository)
		{
			_scenarioRepository = scenarioRepository;
			_existingForecastRepository = existingForecastRepository;
		}

		public IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var existingForecast =  _existingForecastRepository.ExistingForecastForAllSkills(range, scenario);


			return existingForecast.Select(f => new Tuple<string, IEnumerable<DateOnlyPeriod>>(f.Item1, inverse(range, f.Item2).ToArray()))
				.Where(e => e.Item2.Any()).Select(x => new MissingForecastModel
				{
					SkillName = x.Item1,
					MissingRanges =
						x.Item2.Select(y => new MissingForecastRange(){StartDate = y.StartDate.Date,EndDate = y.EndDate.Date}) .ToArray()
				});
		}

		private IEnumerable<DateOnlyPeriod> inverse(DateOnlyPeriod range, IEnumerable<DateOnlyPeriod> raster)
		{
			var result = new List<DateOnlyPeriod>();
			if (!raster.Any())
			{
				result.Add(range);
				return result;
			}
			var previousEnd = range.StartDate.AddDays(-1);
			foreach (var period in raster)
			{
				if (period.StartDate > previousEnd.AddDays(1))
				{
					result.Add(new DateOnlyPeriod(previousEnd.AddDays(1),period.StartDate.AddDays(-1)));
				}
				previousEnd = period.EndDate;
			}
			if (previousEnd < range.EndDate)
				result.Add(new DateOnlyPeriod(previousEnd.AddDays(1), range.EndDate));
			return result;
		}
	}

	public class MissingForecastRange
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}