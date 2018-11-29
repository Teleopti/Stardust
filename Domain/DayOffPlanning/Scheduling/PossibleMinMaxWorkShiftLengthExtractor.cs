using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
    public class PossibleMinMaxWorkShiftLengthExtractor : IPossibleMinMaxWorkShiftLengthExtractor
    {
        private readonly IRestrictionExtractor _restrictionExtractor;
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;
	    private readonly RuleSetBagExtractorProvider _ruleSetBagExtractorProvider;
	    private readonly IDictionary<DateOnly, IWorkTimeMinMax> _extractedLengths = new Dictionary<DateOnly, IWorkTimeMinMax>();

        public PossibleMinMaxWorkShiftLengthExtractor(
            IRestrictionExtractor restrictionExtractor, 
            IWorkShiftWorkTime workShiftWorkTime,
						RuleSetBagExtractorProvider ruleSetBagExtractorProvider)
        {
            _restrictionExtractor = restrictionExtractor;
        	_workShiftWorkTime = workShiftWorkTime;
	        _ruleSetBagExtractorProvider = ruleSetBagExtractorProvider;
        }

        public void ResetCache()
        {
            _extractedLengths.Clear();
        }

        public MinMax<TimeSpan> PossibleLengthsForDate(DateOnly dateOnly, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, OpenHoursSkillResult openHoursSkillResult)
		{
			if (_extractedLengths.Count ==0)
			{
                foreach (var day in matrix.FullWeeksPeriodDays)
                {
                    _extractedLengths.Add(day.Day, null);
                }
			}

			IVirtualSchedulePeriod virtualSchedulePeriod = matrix.SchedulePeriod;

	        IWorkTimeMinMax workTimeMinMax;
	        if (!_extractedLengths.TryGetValue(dateOnly, out workTimeMinMax) || workTimeMinMax == null)
			{
				var result = _restrictionExtractor.Extract(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart());
				IEffectiveRestriction restriction = result.CombinedRestriction(schedulingOptions);

				IWorkTimeMinMax ret = null;
				var person = matrix.Person;
			    var personPeriod = person.Period(dateOnly);

				if (restriction?.Absence != null)
                {
	                var schedulePeriodStartDate = person.SchedulePeriodStartDate(dateOnly);
	                if (schedulePeriodStartDate.HasValue)
	                {
		                if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(schedulePeriodStartDate.Value, dateOnly, person.FirstDayOfWeek) || !restriction.Absence.InContractTime)
			                return new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);

		                return new MinMax<TimeSpan>(personPeriod.PersonContract.AverageWorkTimePerDay, personPeriod.PersonContract.AverageWorkTimePerDay);
					}
				}
				var ruleSetBag = _ruleSetBagExtractorProvider.Fetch(schedulingOptions)
					.GetRuleSetBagForTeamMember(person, dateOnly);

                if (matrix.SchedulePeriod.IsValid && ruleSetBag != null)
				{
					if (openHoursSkillResult != null && openHoursSkillResult.OpenHoursDictionary.TryGetValue(dateOnly, out var startEndRestriction))
					{
						restriction = restriction?.Combine(startEndRestriction);	
					}

					ret = ruleSetBag.MinMaxWorkTime(
						_workShiftWorkTime, dateOnly, restriction);
					_extractedLengths[dateOnly] = ret;
					workTimeMinMax = ret;
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
										workTimeMinMax.WorkTimeLimitation.EndTime.Value);
			}

			return new MinMax<TimeSpan>(workTimeMinMax.WorkTimeLimitation.StartTime.Value,
										workTimeMinMax.WorkTimeLimitation.EndTime.Value);

		}
    }
}