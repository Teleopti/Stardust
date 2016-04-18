using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAbsenceRequestAndSchedules
	{
		IAbsenceRequest AbsenceRequest { get; set; }
		ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
	}

	public class AbsenceRequstAndSchedules : IAbsenceRequestAndSchedules
	{
		public IAbsenceRequest AbsenceRequest { get; set; }
		public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; } 
	}
}