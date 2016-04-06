using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ScheduleIsInvalidSpecification : Specification<ISchedulingResultStateHolder>, IScheduleIsInvalidSpecification
    {
        private readonly static ILog Logger = LogManager.GetLogger(typeof(ScheduleIsInvalidSpecification));

        public override bool IsSatisfiedBy(ISchedulingResultStateHolder obj)
        {
            try
            {
                foreach (KeyValuePair<IPerson, IScheduleRange> scheduleRange in obj.Schedules)
                {
                    var period =
                        scheduleRange.Value.Period.ToDateOnlyPeriod(
                            scheduleRange.Key.PermissionInformation.DefaultTimeZone());
                    foreach (IScheduleDay scheduleDay in scheduleRange.Value.ScheduledDayCollection(period))
                    {
	                    var ass = scheduleDay.PersonAssignment();
											if (ass != null)
											{
												ass.CheckRestrictions();
											}
                    }

                }
            }
            catch (ValidationException validationException)
            {
                Logger.Error(
                    "A validation exception occurred, which probably has with previous errors in schedule to do.",
                    validationException);
                return true;
            }
            return false;
        }
    }
}