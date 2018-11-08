using Teleopti.Ccc.Domain.Budgeting;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestAndSchedules
	{
		IAbsenceRequest AbsenceRequest { get;  }
		ISchedulingResultStateHolder SchedulingResultStateHolder { get; }
		BudgetGroupState BudgetGroupState { get; }
	}

	public interface IBudgetGroupAllowanceSpecification : IPersonRequestSpecification<IAbsenceRequestAndSchedules>
	{

	}
}