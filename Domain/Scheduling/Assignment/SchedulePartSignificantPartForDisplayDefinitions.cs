using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SchedulePartSignificantPartForDisplayDefinitions :SchedulePartSignificantPartDefinitions
    {
        public SchedulePartSignificantPartForDisplayDefinitions(IScheduleDay schedulePart, IHasDayOffDefinition hasDayOffDefinition)
			: base(schedulePart, hasDayOffDefinition)
        { }

        public override bool HasContractDayOff()
        {
			if (HasFullAbsence() && HasDayOff())
				return true;
        	return base.HasContractDayOff();
        }
    }
}
