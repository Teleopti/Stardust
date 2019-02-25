using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class AbsenceStaffingPossibilityCalculator : IAbsenceStaffingPossibilityCalculator
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly INow _now;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillStaffingIntervalUnderstaffing _skillStaffingIntervalUnderStaffing;

		public AbsenceStaffingPossibilityCalculator(IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository,
			ISkillStaffingDataLoader skillStaffingDataLoader,
			INow now,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillStaffingIntervalUnderstaffing skillStaffingIntervalUnderStaffing)
		{
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_now = now;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillStaffingIntervalUnderStaffing = skillStaffingIntervalUnderStaffing;
		}

		public CalculatedPossibilityModelResult CalculateIntradayIntervalPossibilities(IPerson person, DateOnlyPeriod period)
		{
			var periodWithYesterdayAndToday = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate);
			var scheduleDictionary = loadScheduleDictionary(person, periodWithYesterdayAndToday);

			var skills = getSupportedPersonSkills(person, period).Select(s => s.Skill).ToArray();
			var useShrinkageDic = getShrinkageStatusAccordingToPeriods(person, period);
			var workflowControlSet = person.WorkflowControlSet;
			var skillStaffingData = _skillStaffingDataLoader.Load(skills, period, useShrinkageDic, date =>
				workflowControlSet?.AbsenceRequestOpenPeriods != null &&
				workflowControlSet.AbsenceRequestOpenPeriods.Any() &&
				workflowControlSet.IsAbsenceRequestCheckStaffingByIntraday(_now.CurrentLocalDate(person.PermissionInformation.DefaultTimeZone()), date));

			return new CalculatedPossibilityModelResult(scheduleDictionary, calculatePossibilities(person, skillStaffingData, scheduleDictionary));
		}

		private Dictionary<DateOnly, bool> getShrinkageStatusAccordingToPeriods(IPerson person, DateOnlyPeriod period)
		{
			var today = _now.CurrentLocalDate(person.PermissionInformation.DefaultTimeZone());
			var useShrinkageDic = period.DayCollection().ToDictionary(d => d, dateOnly =>
				person.WorkflowControlSet?.AbsenceRequestOpenPeriods != null && person.WorkflowControlSet
					.IsAbsenceRequestCheckStaffingByIntradayWithShrinkage(
						today, dateOnly));

			return useShrinkageDic;
		}

		private IScheduleDictionary loadScheduleDictionary(IPerson person, DateOnlyPeriod period)
		{
			var loadOption = new ScheduleDictionaryLoadOptions(false, false) { LoadAgentDayScheduleTags = false };
			var scenario = _scenarioRepository.Current();

			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person }, loadOption, period, scenario);
			return scheduleDictionary;
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(IPerson person,
			IList<SkillStaffingData> skillStaffingData, IScheduleDictionary scheduleDictionary)
		{
			var resolution = skillStaffingData.FirstOrDefault()?.Resolution ?? 15;
			var skillStaffingDataGroups = skillStaffingData.GroupBy(s => s.Date);
			return skillStaffingDataGroups.Select(skillStaffingDataGroup => new CalculatedPossibilityModel
			{
				Date = skillStaffingDataGroup.Key,
				IntervalPossibilies =
					calculateIntervalPossibilities(person, skillStaffingDataGroup.ToList(), scheduleDictionary),
				Resolution = resolution
			}).ToList();
		}

		private Dictionary<DateTime, int> calculateIntervalPossibilities(IPerson person, IList<SkillStaffingData> skillStaffingData, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();

			foreach (var skillStaffing in skillStaffingData)
			{
				if (!staffingDataHasValue(skillStaffing))
					continue;

				if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffing.Time))
					continue;

				if (isSkillScheduledInThisInterval(person, skillStaffing, scheduleDictionary))
				{
					subtractUsersSchedule(skillStaffing);
				}

				var possibility = calculatePossibility(skillStaffing);
				var key = skillStaffing.Time;
				intervalPossibilities[key] = possibility;
			}
			return intervalPossibilities;
		}

		private int calculatePossibility(SkillStaffingData skillStaffingData)
		{
			var isSatisfied = _skillStaffingIntervalUnderStaffing.IsSatisfiedBy(skillStaffingData.Skill, skillStaffingData.SkillStaffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.FairPossibility : ScheduleStaffingPossibilityConsts.GoodPossibility;
		}

		private static bool isSkillScheduled(IPersonAssignment personAssignment, DateTimePeriod period, ISkill skill)
		{
			if (isPersonAssignmentNullOrEmpty(personAssignment))
				return false;

			var mainActivities = personAssignment.MainActivities();
			var overtimeActivities = personAssignment.OvertimeActivities();

			var isSkillScheduled =
				mainActivities
					.Any(m => m.Payload.RequiresSkill && m.Payload == skill.Activity && m.Period.Intersect(period))
				|| overtimeActivities
					.Any(m => m.Payload.RequiresSkill && m.Payload == skill.Activity && m.Period.Intersect(period));

			return isSkillScheduled;
		}

		private static IPersonAssignment getPersonAssignment(IScheduleDictionary scheduleDictionary, DateOnly date)
		{
			var scheduleDays = scheduleDictionary.SchedulesForDay(date);
			var scheduleDay = scheduleDays.FirstOrDefault();
			var personAssignment = scheduleDay?.PersonAssignment();
			return personAssignment;
		}

		private static bool isPersonAssignmentNullOrEmpty(IPersonAssignment personAssignment)
		{
			return personAssignment == null || personAssignment.ShiftLayers.IsEmpty();
		}

		private bool isSkillScheduledInThisInterval(IPerson person, SkillStaffingData skillStaffingData, IScheduleDictionary scheduleDictionary)
		{
			var scheduleDate = skillStaffingData.Date;
			var personAssignmentDictionary = new Dictionary<DateOnly, IPersonAssignment>
			{
				{scheduleDate.AddDays(-1), getPersonAssignment(scheduleDictionary, scheduleDate.AddDays(-1))},
				{scheduleDate, getPersonAssignment(scheduleDictionary, scheduleDate)}
			};
			var personAssignmentToday = personAssignmentDictionary[scheduleDate];
			var personAssignmentYesterday = personAssignmentDictionary[scheduleDate.AddDays(-1)];

			var skill = skillStaffingData.Skill;
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var startTime = TimeZoneHelper.ConvertToUtc(skillStaffingData.Time, timezone);
			var skillScheduled = isSkillScheduled(personAssignmentToday, new DateTimePeriod(startTime, startTime.AddMinutes(skillStaffingData.Resolution)),skill)
								 || isSkillScheduled(personAssignmentYesterday, new DateTimePeriod(startTime, startTime.AddMinutes(skillStaffingData.Resolution)),skill);
			return skillScheduled;
		}

		private void subtractUsersSchedule(SkillStaffingData skillStaffingData)
		{
			if (skillStaffingData.ScheduledStaffing <= 1)
			{
				skillStaffingData.ScheduledStaffing = 0;
			}
			else
			{
				skillStaffingData.ScheduledStaffing -= 1;
			}
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			return intervalPossibilities.TryGetValue(time, out var possibility) && possibility == ScheduleStaffingPossibilityConsts.FairPossibility;
		}

		private static bool staffingDataHasValue(SkillStaffingData skillStaffingData)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (personPeriod.Length == 0)
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return personSkills.Distinct();
		}
	}
}