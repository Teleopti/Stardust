using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WorkTimeMinMaxCalculator : IWorkTimeMinMaxCalculator
	{
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private readonly IEffectiveRestrictionForDisplayCreator _effectiveRestrictionCreator;

		public WorkTimeMinMaxCalculator(IWorkShiftWorkTime workShiftWorkTime, IEffectiveRestrictionForDisplayCreator effectiveRestrictionCreator)
		{
			_workShiftWorkTime = workShiftWorkTime;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IWorkTimeMinMax WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay, out PreferenceType? preferenceType)
		{
			if (person == null) throw new ArgumentNullException("person");
			preferenceType = null;
			var personPeriod = person.PersonPeriods(new DateOnlyPeriod(date, date)).SingleOrDefault();
			if (personPeriod == null)
				return null;

			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null)
				return null;

			var options = new EffectiveRestrictionOptions(true, true);

			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestrictionForDisplay(scheduleDay, options);
			if (effectiveRestriction != null)
			{
				if (effectiveRestriction.DayOffTemplate != null)
				{
					preferenceType = PreferenceType.DayOff;
				}
				else if (effectiveRestriction.Absence != null)
				{
					preferenceType = PreferenceType.Absence;
				}
				else if (effectiveRestriction.ShiftCategory != null)
				{
					preferenceType = PreferenceType.ShiftCategory;
				}
			}

			if (effectiveRestriction != null && effectiveRestriction.Absence != null)
			{
				return WorkTimeMinMaxForAbsence(scheduleDay, effectiveRestriction);
			}

			return ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, date, effectiveRestriction);
		}

		private static IWorkTimeMinMax WorkTimeMinMaxForAbsence(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
		{
			var person = scheduleDay.Person;
			var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = person.Period(scheduleDate);
			var personContract = personPeriod.PersonContract;
			var avgWorkTime = new TimeSpan((long)(person.AverageWorkTimeOfDay(scheduleDate).Ticks * personContract.PartTimePercentage.Percentage.Value));

			if (!personContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate))
				return null;

			if (!effectiveRestriction.Absence.InContractTime)
				return null;

			return new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime)};
		}
	}

	
}