using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class LongestPeriodForAssignmentCalculator : ILongestPeriodForAssignmentCalculator
    {
        public DateTimePeriod? PossiblePeriod(IScheduleRange current, DateOnly dateToCheck)
        {
            var approximateTime = dateToCheck.Date.AddHours(12);
            var approxUtc = TimeZoneHelper.ConvertToUtc(approximateTime,
                                                             current.Person.PermissionInformation.DefaultTimeZone());
	        var rules = new IAssignmentPeriodRule[]
	        {
		        new OverlappingAssignmentRule(),
		        new DayOffRule(),
		        new NightlyRestRule()
	        };
            DateTimePeriod? returnPeriod = new DateTimePeriod(approxUtc.AddDays(-2), approxUtc.AddDays(2));
            foreach (var rule in rules)
            {
                var tmpPeriod = rule.LongestDateTimePeriodForAssignment(current, dateToCheck);
                returnPeriod = returnPeriod.Value.Intersection(tmpPeriod);
                if (!returnPeriod.HasValue) break;
            }
            return returnPeriod;
        }
    }
}
