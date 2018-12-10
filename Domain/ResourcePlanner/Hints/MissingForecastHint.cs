using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class MissingForecastHint : ISchedulePreHint
	{
		private readonly IExistingForecastRepository _existingForecastRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public MissingForecastHint(IExistingForecastRepository existingForecastRepository, IScenarioRepository scenarioRepository)
		{
			_existingForecastRepository = existingForecastRepository;
			_scenarioRepository = scenarioRepository;
		}

		public IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range)
		{
			var existingForecast = _existingForecastRepository.ExistingForecastForAllSkills(range, _scenarioRepository.LoadDefaultScenario());
			return existingForecast?.Select(skillMissingForecast => new MissingForecastModel
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
					   .OrderBy(missingForecastModel => missingForecastModel.SkillName) ?? Enumerable.Empty<MissingForecastModel>();
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

		public void FillResult(HintResult hintResult, ScheduleHintInput input)
		{
			var people = input.People;
			var range = input.Period;
			var missingForecasts = GetMissingForecast(range).ToList();
			var skills = new HashSet<ISkill>();
			foreach (var periods in people.Select(person => person.PersonPeriods(range)))
				foreach (var period in periods)
					foreach (var personSkill in period.PersonSkillCollection)
						skills.Add(personSkill.Skill);
			foreach (var missingForecast in missingForecasts.Where(missingForecast => skills.Any(skill => skill.Id == missingForecast.SkillId)))
			{
				hintResult.Add(missingForecast, GetType());
			}
		}
	}

	public class MissingForecastRange
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}