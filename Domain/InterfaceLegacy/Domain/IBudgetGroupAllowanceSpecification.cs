namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestAndSchedules
	{
		IAbsenceRequest AbsenceRequest { get; set; }
		ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
	}

	public interface IBudgetGroupAllowanceSpecification : IPersonRequestSpecification<IAbsenceRequestAndSchedules>
	{

	}
}