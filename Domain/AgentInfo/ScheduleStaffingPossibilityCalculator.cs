using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private const int goodPossibility = 1;
		private const int fairPossibility = 0;

		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICacheableStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;

		public ScheduleStaffingPossibilityCalculator(INow now, ILoggedOnUser loggedOnUser,
			ICacheableStaffingViewModelCreator staffingViewModelCreator, IScheduleStorage scheduleStorage, ICurrentScenario scenarioRepository)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
			_staffingViewModelCreator = staffingViewModelCreator;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
		}

		public IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities()
		{
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasUnderstaffing(skill);
			return calcuateIntervalPossibilities(skillStaffingDatas, getStaffingSpecification);
		}

		public IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities()
		{
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasOverstaffing(skill);
			return calcuateIntervalPossibilities(skillStaffingDatas, getStaffingSpecification);
		}

		private IEnumerable<skillStaffingData> getSkillStaffingData(bool useShrinkage)
		{
			var personSkills = getPersonSkills().ToList();

			return from skill in personSkills.Select(x => x.Skill)
					let intradayStaffingModel = _staffingViewModelCreator.Load(skill.Id.GetValueOrDefault(), useShrinkage)
					select new skillStaffingData
					{
						Skill = skill,
						Time = intradayStaffingModel.DataSeries?.Time,
						ForecastedStaffing = intradayStaffingModel.DataSeries?.ForecastedStaffing,
						ScheduledStaffing = intradayStaffingModel.DataSeries?.ScheduledStaffing
					};
		}

		private IEnumerable<IPersonSkill> getPersonSkills()
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod =
				person.PersonPeriodCollection.FirstOrDefault(x => x.Period.Contains(new DateOnly(_now.UtcDateTime())));
			if (personPeriod == null)
				return new IPersonSkill[] {};
			var personSkills = personPeriod.PersonSkillCollection?.ToList();
			if (personSkills == null || !personSkills.Any())
				return new IPersonSkill[] {};

			var intradaySchedule = getIntradaySchedule();
			var personAssignment = intradaySchedule?.PersonAssignment();
			if (personAssignment == null || personAssignment.ShiftLayers.IsEmpty())
				return personSkills;

			var scheduledActivities = personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill);
			return personSkills.Where(p => scheduledActivities.Contains(p.Skill.Activity));
		}

		private IScheduleDay getIntradaySchedule()
		{
			var date = new DateOnly(_now.UtcDateTime());
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {_loggedOnUser.CurrentUser()},
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
				defaultScenario);

			var scheduleDays = dictionary.SchedulesForDay(date).ToList();
			return scheduleDays.Any() ? scheduleDays.First() : null;
		}

		private static Dictionary<DateTime, int> calcuateIntervalPossibilities(IEnumerable<skillStaffingData> skillStaffingDatas,
		Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				var staffingSpecification = getStaffingSpecification(skillStaffingData.Skill);

				for (var i = 0; i < skillStaffingData.Time?.Length; i++)
				{
					if (!staffingDataHasValue(skillStaffingData, i)) continue;

					var staffingInterval = new SkillStaffingInterval
					{
						CalculatedResource = skillStaffingData.ScheduledStaffing[i].Value,
						FStaff = skillStaffingData.ForecastedStaffing[i].Value
					};

					if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time[i]))
						continue;

					var isSatisfied = staffingSpecification.IsSatisfiedBy(staffingInterval);
					var possibility = isSatisfied ? fairPossibility : goodPossibility;
					var key = skillStaffingData.Time[i];
					if (intervalPossibilities.ContainsKey(key))
					{
						intervalPossibilities[key] = possibility;
					}
					else
					{
						intervalPossibilities.Add(key, possibility);
					}
				}
			}
			return intervalPossibilities;
		}

		private bool isShrinkageValidatorEnabled()
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				return false;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			return person.WorkflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdWithShrinkageValidator>(timeZone);
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			int possibility;
			return intervalPossibilities.TryGetValue(time, out possibility) && possibility == fairPossibility;
		}

		private static bool staffingDataHasValue(skillStaffingData skillStaffingData, int i)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.Length > i &&
												   skillStaffingData.ScheduledStaffing[i].HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.Length > i &&
													skillStaffingData.ForecastedStaffing[i].HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}

		private class skillStaffingData
		{
			public ISkill Skill { get; set; }
			public DateTime[] Time { get; set; }
			public double?[] ForecastedStaffing { get; set; }
			public double?[] ScheduledStaffing { get; set; }
		}
	}
}
