using System;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WorkTimeMinMaxCalculator : IWorkTimeMinMaxCalculator
	{
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private readonly IWorkTimeMinMaxRestrictionCreator _workTimeMinMaxRestrictionCreator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShiftWork")]
        public WorkTimeMinMaxCalculator(IWorkShiftWorkTime workShiftWorkTime, IWorkTimeMinMaxRestrictionCreator workTimeMinMaxRestrictionCreator)
		{
			_workShiftWorkTime = workShiftWorkTime;
			_workTimeMinMaxRestrictionCreator = workTimeMinMaxRestrictionCreator;
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay)
		{
			return WorkTimeMinMax(date, person, scheduleDay, EffectiveRestrictionOptions.UseAll());
		}

		public WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay, IEffectiveRestrictionOptions option)
		{
			if (person == null) throw new ArgumentNullException("person");
			var result = new WorkTimeMinMaxCalculationResult();

			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				return null;

			var createdRestriction = _workTimeMinMaxRestrictionCreator.MakeWorkTimeMinMaxRestriction(scheduleDay, option);
			if (createdRestriction.Restriction != null)
				result.RestrictionNeverHadThePossibilityToMatchWithShifts = !createdRestriction.Restriction.MayMatchWithShifts();
			if (createdRestriction.IsAbsenceInContractTime)
			{
				result.WorkTimeMinMax = WorkTimeMinMaxForAbsence(scheduleDay);
				return result;
			}

			createdRestriction.Restriction = new PersonalShiftRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, (IEffectiveRestriction)createdRestriction.Restriction);
			createdRestriction.Restriction = new MeetingRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, (IEffectiveRestriction)createdRestriction.Restriction);

			result.WorkTimeMinMax = ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, date, createdRestriction.Restriction);
			return result;
		}

		private static IWorkTimeMinMax WorkTimeMinMaxForAbsence(IScheduleDay scheduleDay)
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

	
}