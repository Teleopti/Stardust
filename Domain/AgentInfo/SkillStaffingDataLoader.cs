using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingDataLoader : ISkillStaffingDataLoader
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public SkillStaffingDataLoader(ILoggedOnUser loggedOnUser, ScheduledStaffingProvider scheduledStaffingProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ICurrentScenario scenarioRepository, ISkillDayRepository skillDayRepository,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_scenarioRepository = scenarioRepository;
			_skillDayRepository = skillDayRepository;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IList<SkillStaffingData> Load(DateOnlyPeriod period)
		{
			var personSkills = getSupportedPersonSkills(period).ToArray();

			var skillStaffingList = new List<SkillStaffingData>();
			if (!personSkills.Any()) return skillStaffingList;

			var resolution = personSkills.Min(s => s.Skill.DefaultResolution);
			var useShrinkage = isShrinkageValidatorEnabled();
			var skillDays = _skillDayRepository.FindReadOnlyRange(period.Inflate(1), personSkills.Select(s => s.Skill),
				_scenarioRepository.Current()).ToList();

			var intradayStaffingModels = createSkillStaffingDatas(period, personSkills.Select(s => s.Skill).ToList(), resolution, useShrinkage, skillDays);

			skillStaffingList.AddRange(intradayStaffingModels);


			return skillStaffingList;
		}

		private IList<SkillStaffingData> createSkillStaffingDatas(DateOnlyPeriod period, IList<ISkill> skills, int resolution, bool useShrinkage,
			IList<ISkillDay> skillDays)
		{
			var dayStaffingDatas =
				period.DayCollection()
					.Select(
						day =>
							new
							{
								Date = day,
								Scheduled =
								_scheduledStaffingProvider.StaffingPerSkill(skills, resolution, day, useShrinkage)
									.ToLookup(x => new { x.StartDateTime, x.Id}),
								Forecasted =
								_forecastedStaffingProvider.StaffingPerSkill(skills, skillDays.Where(s=>s.CurrentDate == day).ToArray(), resolution, day,
									useShrinkage).ToLookup(x => new {x.StartTime, x.SkillId})
							});
			var skillStaffingDatas = from dayStaffingData in dayStaffingDatas
				let p = dayStaffingData.Date
				let times =
				dayStaffingData.Scheduled.Select(t => t.Key.StartDateTime).Union(dayStaffingData.Forecasted.Select(t => t.Key.StartTime)).Distinct().OrderBy(t => t).ToArray()
				from t in times
				from skill in skills
				let scheduled = dayStaffingData.Scheduled[new {StartDateTime = t, Id = skill.Id.GetValueOrDefault()}].FirstOrDefault()
				select new SkillStaffingData
				{
					Resolution = resolution,
					Date = p,
					Skill = skill,
					Time = t,
					ForecastedStaffing = dayStaffingData.Forecasted[new { StartTime = t, SkillId = skill.Id.GetValueOrDefault() }].FirstOrDefault()?.Agents,
					ScheduledStaffing = scheduled.StartDateTime == new DateTime() ? null : (double?)scheduled.StaffingLevel
				};
			return skillStaffingDatas.ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private bool isShrinkageValidatorEnabled()
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				return false;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			return person.WorkflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdWithShrinkageValidator>(timeZone);
		}
	}

	public interface ISkillStaffingDataLoader
	{
		IList<SkillStaffingData> Load(DateOnlyPeriod period);
	}
}