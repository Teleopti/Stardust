using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class IsSignificantPartFullDayAbsenceOrDayOffSpecification : Specification<SchedulePartView>
    {
        public override bool IsSatisfiedBy(SchedulePartView obj)
        {
            return obj == SchedulePartView.FullDayAbsence ||
                   obj == SchedulePartView.DayOff ||
                   obj == SchedulePartView.ContractDayOff;
        }
    }
}