using System;
using System.Linq;
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

		public WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay)
		{
			if (person == null) throw new ArgumentNullException("person");
			var result = new WorkTimeMinMaxCalculationResult();

			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				return null;

			var createdRestriction = _workTimeMinMaxRestrictionCreator.MakeWorkTimeMinMaxRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll());
			if (createdRestriction.Restriction != null)
				result.RestrictionNeverHadThePossibilityToMatchWithShifts = !createdRestriction.Restriction.MayMatchWithShifts();
			if (createdRestriction.IsAbsenceInContractTime)
			{
				result.WorkTimeMinMax = WorkTimeMinMaxForAbsence(scheduleDay);
				return result;
			}

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

			if (!personContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate))
				return null;

			return new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime)};
		}
	}

	
}