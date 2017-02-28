using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class AssignScheduledLayers
	{
		public void Execute(ISchedulingOptions schedulingOptions, IScheduleDay scheduleDay, IEditableShift shiftToAssign)
		{
			if (schedulingOptions.OvertimeType != null)
			{
				foreach (var editableShiftLayer in shiftToAssign.LayerCollection)
				{
					scheduleDay.CreateAndAddOvertime(editableShiftLayer.Payload, editableShiftLayer.Period, schedulingOptions.OvertimeType);
				}		
			}
			else
			{
				scheduleDay.AddMainShift(shiftToAssign);
			}	
		}
	}
}