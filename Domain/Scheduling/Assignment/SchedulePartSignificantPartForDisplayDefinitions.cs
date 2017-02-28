using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SchedulePartSignificantPartForDisplayDefinitions :SchedulePartSignificantPartDefinitions
    {
        public SchedulePartSignificantPartForDisplayDefinitions(IScheduleDay schedulePart, IHasContractDayOffDefinition hasContractDayOffDefinition)
			: base(schedulePart, hasContractDayOffDefinition)
        { }

        public override bool HasContractDayOff()
        {
			if (HasFullAbsence() && HasDayOff())
				return true;
        	return base.HasContractDayOff();
        }
    }
}
