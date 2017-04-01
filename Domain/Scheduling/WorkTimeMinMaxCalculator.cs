using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WorkTimeMinMaxCalculator : IWorkTimeMinMaxCalculator
	{
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private readonly IWorkTimeMinMaxRestrictionCreator _workTimeMinMaxRestrictionCreator;

		public WorkTimeMinMaxCalculator(IWorkShiftWorkTime workShiftWorkTime, IWorkTimeMinMaxRestrictionCreator workTimeMinMaxRestrictionCreator)
		{
			_workShiftWorkTime = workShiftWorkTime;
			_workTimeMinMaxRestrictionCreator = workTimeMinMaxRestrictionCreator;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IRuleSetBag ruleSetBag, IScheduleDay scheduleDay)
		{
			return WorkTimeMinMax(date, ruleSetBag, scheduleDay, new EffectiveRestrictionOptions
			{
				UseAvailability = true,
				UsePreference = true
			});
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IRuleSetBag ruleSetBag, IScheduleDay scheduleDay, IEffectiveRestrictionOptions option)
		{
			var result = new WorkTimeMinMaxCalculationResult();
			if (ruleSetBag == null) return null;

			var createdRestriction = _workTimeMinMaxRestrictionCreator.MakeWorkTimeMinMaxRestriction(scheduleDay, option);
			if (createdRestriction.Restriction != null)
				result.RestrictionNeverHadThePossibilityToMatchWithShifts = !createdRestriction.Restriction.MayMatchWithShifts();
			if (createdRestriction.IsAbsenceInContractTime)
			{
				result.WorkTimeMinMax = workTimeMinMaxForAbsence(scheduleDay);
				return result;
			}

			createdRestriction.Restriction = new PersonalShiftRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, (IEffectiveRestriction)createdRestriction.Restriction);
			createdRestriction.Restriction = new MeetingRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, (IEffectiveRestriction)createdRestriction.Restriction);

			result.WorkTimeMinMax = ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, date, createdRestriction.Restriction);
			return result;
		}

		private static IWorkTimeMinMax workTimeMinMaxForAbsence(IScheduleDay scheduleDay)
		{
			var person = scheduleDay.Person;
			var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var averageWorkTimeOfDay = person.AverageWorkTimeOfDay(scheduleDate);
			var avgWorkTime = new TimeSpan((long)(averageWorkTimeOfDay.AverageWorkTime.Value.Ticks * averageWorkTimeOfDay.PartTimePercentage.Value));

			if (!averageWorkTimeOfDay.IsWorkDay) return null;

			return new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime)
			};
		}
	}

	public interface IPersonRuleSetBagProvider
	{
		IRuleSetBag ForDate(IPerson person, DateOnly date);
		IDictionary<DateOnly, IRuleSetBag> ForPeriod(IPerson person, DateOnlyPeriod period);
	}

	public class PersonRuleSetBagProvider : IPersonRuleSetBagProvider
	{
		private readonly IRuleSetBagRepository _repository;

		public PersonRuleSetBagProvider(IRuleSetBagRepository repository)
		{
			_repository = repository;
		}

		public IRuleSetBag ForDate(IPerson person, DateOnly date)
		{
			var personPeriod = person?.Period(date);
			var ruleSetBag = personPeriod?.RuleSetBag;
			return ruleSetBag != null ? _repository.FindWithRuleSetsAndAccessibility(ruleSetBag.Id.GetValueOrDefault()) : null;
		}

		public IDictionary<DateOnly, IRuleSetBag> ForPeriod(IPerson person, DateOnlyPeriod period)
		{
			var ruleBagIdList = new List<Guid>();
			var allRuleSetBagIds = new Dictionary<DateOnly, Guid?>();
			foreach (var date in period.DayCollection())
			{
				var personPeriod = person?.Period(date);
				var ruleSetBag = personPeriod?.RuleSetBag;
				if (ruleSetBag?.Id != null && !ruleBagIdList.Contains(ruleSetBag.Id.Value))
				{
					ruleBagIdList.Add(ruleSetBag.Id.Value);
				}
				allRuleSetBagIds.Add(date, ruleSetBag?.Id);
			}

			var allRuleSetBags = ruleBagIdList.Any()
				? _repository.FindWithRuleSetsAndAccessibility(ruleBagIdList.ToArray()).ToList()
				: new List<IRuleSetBag>();

			return allRuleSetBagIds.ToDictionary(item => item.Key, item => allRuleSetBags.SingleOrDefault(b => b.Id == item.Value));
		}
	}
}