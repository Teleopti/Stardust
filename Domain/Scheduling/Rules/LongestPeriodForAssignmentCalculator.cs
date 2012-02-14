using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using System;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class LongestPeriodForAssignmentCalculator : ILongestPeriodForAssignmentCalculator
    {
        private readonly IList<IAssignmentPeriodRule> _rules;

        public LongestPeriodForAssignmentCalculator()
        {
            _rules = new List<IAssignmentPeriodRule> {
                                                    new OverlappingAssignmentRule(),
                                                    new DayOffRule(),
                                                    new NightlyRestRule()
                                                };
        }

        public DateTimePeriod? PossiblePeriod(IScheduleRange current, DateOnly dateToCheck)
        {
            DateTime approximateTime = new DateTime(dateToCheck.Year, dateToCheck.Month, dateToCheck.Day, 12, 0, 0, DateTimeKind.Unspecified);
            DateTime approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime,
                                                             current.Person.PermissionInformation.DefaultTimeZone());
            DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));
            foreach (var rule in _rules)
            {
                DateTimePeriod tmpPeriod = rule.LongestDateTimePeriodForAssignment(current, dateToCheck);
                returnPeriod = returnPeriod.Value.Intersection(tmpPeriod);
                if (!returnPeriod.HasValue) break;
            }
            return returnPeriod;
        }
    }
}
