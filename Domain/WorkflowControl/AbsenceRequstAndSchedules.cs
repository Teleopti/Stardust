using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequstAndSchedules : IAbsenceRequestAndSchedules
	{
		public IAbsenceRequest AbsenceRequest { get; set; }
		public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; } 
	}
}