using System;
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
			if (ruleSetBag == null) throw new ArgumentNullException("ruleSetBag");

			var result = new WorkTimeMinMaxCalculationResult();

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
			var personPeriod = person.Period(scheduleDate);
			var personContract = personPeriod.PersonContract;
			var avgWorkTime = new TimeSpan((long)(person.AverageWorkTimeOfDay(scheduleDate).Ticks * personContract.PartTimePercentage.Percentage.Value));

			var schedulePeriod = person.VirtualSchedulePeriod(scheduleDate);

			if (!personContract.ContractSchedule.IsWorkday(schedulePeriod.DateOnlyPeriod.StartDate, scheduleDate))
				return null;

			return new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime)};
		}
	}

	public interface IPersonRuleSetBagProvider
	{
		IRuleSetBag ForDate(IPerson person, DateOnly date);
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
			if (person == null) return null;

			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			return ruleSetBag != null ? _repository.Find(ruleSetBag.Id.GetValueOrDefault()) : null;
		}
	}
}