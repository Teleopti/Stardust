using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret.Scheduling
{
    public class PossibleMinMaxWorkShiftLengthExtractor : IPossibleMinMaxWorkShiftLengthExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;
        private readonly IDictionary<DateOnly, IWorkTimeMinMax> _extractedLengths = new Dictionary<DateOnly, IWorkTimeMinMax>();

        public PossibleMinMaxWorkShiftLengthExtractor(
            IRestrictionExtractor restrictionExtractor, 
            IWorkShiftWorkTime workShiftWorkTime)
        {
            _restrictionExtractor = restrictionExtractor;
        	_workShiftWorkTime = workShiftWorkTime;
        }

        public void ResetCache()
        {
            _extractedLengths.Clear();
        }

        public MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
		{
			if (_extractedLengths.Count ==0)
			{
                foreach (var day in matrix.FullWeeksPeriodDays)
                {
                    _extractedLengths.Add(day.Day, null);
                }
			}

			IVirtualSchedulePeriod virtualSchedulePeriod = matrix.SchedulePeriod;

			if (_extractedLengths[dateOnly] == null)
			{
				_restrictionExtractor.Extract(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart());
				IEffectiveRestriction restriction = _restrictionExtractor.CombinedRestriction(schedulingOptions);

				IWorkTimeMinMax ret = null;
				var person = matrix.Person;
			    var personPeriod = person.Period(dateOnly);
				var schedulePeriodStartDate = person.SchedulePeriodStartDate(dateOnly);

				if (restriction != null && restriction.Absence != null && schedulePeriodStartDate.HasValue)
                {
					if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(schedulePeriodStartDate.Value, dateOnly) || !restriction.Absence.InContractTime)
                        return new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);

                    return new MinMax<TimeSpan>(personPeriod.PersonContract.AverageWorkTimePerDay, personPeriod.PersonContract.AverageWorkTimePerDay);
                }
			    IRuleSetBag ruleSetBag = null;
                if(personPeriod != null)
			        ruleSetBag = personPeriod.RuleSetBag;

                if (matrix.SchedulePeriod.IsValid && ruleSetBag != null)
				{
                    ret = ruleSetBag.MinMaxWorkTime(
						_workShiftWorkTime, dateOnly, restriction);
					_extractedLengths[dateOnly] = ret;
				}
				if (ret == null)
				{
					return new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);
				}
			}

            var contract = virtualSchedulePeriod.Contract;
			EmploymentType empType = contract.EmploymentType;

			if (empType == EmploymentType.HourlyStaff)
			{
				return new MinMax<TimeSpan>(TimeSpan.Zero,
										_extractedLengths[dateOnly].WorkTimeLimitation.EndTime.Value);
			}

			return new MinMax<TimeSpan>(_extractedLengths[dateOnly].WorkTimeLimitation.StartTime.Value,
										_extractedLengths[dateOnly].WorkTimeLimitation.EndTime.Value);

		}
    }
}