using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
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

			return existingForecast
				.Select(skillMissingForecast => new MissingForecastModel
				{
					SkillName = skillMissingForecast.SkillName,
					MissingRanges = inverse(range, skillMissingForecast.Periods).Select(r => new MissingForecastRange
					{
						StartDate = r.StartDate.Date,
						EndDate = r.EndDate.Date
					}).ToArray(),
					SkillId = skillMissingForecast.SkillId
				})
				.Where(missingForecastModel => missingForecastModel.MissingRanges.Any())
				.OrderBy(missingForecastModel => missingForecastModel.SkillName);
		}

		private static IEnumerable<DateOnlyPeriod> inverse(DateOnlyPeriod range, IEnumerable<DateOnlyPeriod> raster)
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

		public IEnumerable<MissingForecastModel> GetMissingForecast(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var missingForecasts = GetMissingForecast(range);
			var skills = new HashSet<ISkill>();
			foreach (var periods in people.Select(person => person.PersonPeriods(range)))
				foreach (var period in periods)
					foreach (var personSkill in period.PersonSkillCollection)
						skills.Add(personSkill.Skill);
			return missingForecasts.Where(missingForecast => skills.Any(skill => skill.Id == missingForecast.SkillId)).ToList();
		}
	}

	public class MissingForecastRange
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}