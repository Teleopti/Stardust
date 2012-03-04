using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{
    public class PossibleMinMaxWorkShiftLengthExtractor : IPossibleMinMaxWorkShiftLengthExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;
        private readonly IRuleSetProjectionService _ruleSetProjectionService;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IDictionary<DateOnly, IWorkTimeMinMax> _extractedLengths = new Dictionary<DateOnly, IWorkTimeMinMax>();

        public PossibleMinMaxWorkShiftLengthExtractor(
            IRestrictionExtractor restrictionExtractor, 
            IRuleSetProjectionService ruleSetProjectionService, 
            ISchedulingOptions schedulingOptions)
        {
            _restrictionExtractor = restrictionExtractor;
            _ruleSetProjectionService = ruleSetProjectionService;
            _schedulingOptions = schedulingOptions;
        }

        public void ResetCache()
        {
            _extractedLengths.Clear();
        }

        public MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix)
		{
			if (_extractedLengths.Count ==0)
			{
                foreach (var day in matrix.FullWeeksPeriodDays)
                {
                    _extractedLengths.Add(day.Day, null);
                }
			}

			IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
			if (_extractedLengths[dateOnly] == null)
			{
				_restrictionExtractor.Extract(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart());
				IEffectiveRestriction restriction = _restrictionExtractor.CombinedRestriction(_schedulingOptions);

				IWorkTimeMinMax ret = null;
			    var personPeriod = schedulePeriod.Person.Period(dateOnly);
                if(restriction != null && restriction.Absence != null)
                {
                    if(!personPeriod.PersonContract.ContractSchedule.IsWorkday(personPeriod.StartDate, dateOnly) || !restriction.Absence.InContractTime)
                        return new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);

                    return new MinMax<TimeSpan>(personPeriod.PersonContract.AverageWorkTimePerDay, personPeriod.PersonContract.AverageWorkTimePerDay);
                }
			    IRuleSetBag ruleSetBag = null;
                if(personPeriod != null)
			        ruleSetBag = personPeriod.RuleSetBag;

                if (matrix.SchedulePeriod.IsValid && ruleSetBag != null)
				{
                    ret = ruleSetBag.MinMaxWorkTime(
						_ruleSetProjectionService, dateOnly, restriction);
					_extractedLengths[dateOnly] = ret;
				}
				if (ret == null)
				{
					return new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);
				}
			}

            var contract = schedulePeriod.Contract;
			//IContract contract = schedulePeriod.PersonPeriod.PersonContract.Contract;
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