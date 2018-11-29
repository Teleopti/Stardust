using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class SchedulePublishedSpecification : Specification<DateOnly>
    {
        private readonly IWorkflowControlSet _workflowControlSet;
        private readonly ScheduleVisibleReasons _scheduleVisibleReasons;

        public SchedulePublishedSpecification(IWorkflowControlSet workflowControlSet, ScheduleVisibleReasons scheduleVisibleReasons)
        {
            _workflowControlSet = workflowControlSet;
            _scheduleVisibleReasons = scheduleVisibleReasons;
        }

        public override bool IsSatisfiedBy(DateOnly obj)
        {
            if (_workflowControlSet == null) return false;
            if ((_scheduleVisibleReasons & ScheduleVisibleReasons.Preference) == ScheduleVisibleReasons.Preference)
            {
                if (_workflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today) &&
                    _workflowControlSet.PreferencePeriod.Contains(obj))
                {
                    return true;
                }
            }

            if ((_scheduleVisibleReasons & ScheduleVisibleReasons.Published) == ScheduleVisibleReasons.Published)
            {
                if (_workflowControlSet.SchedulePublishedToDate.HasValue && new DateOnly(_workflowControlSet.SchedulePublishedToDate.Value).AddDays(1) > obj)
                {
                    return true;
                }
            }
            return false;
        }
    }
}