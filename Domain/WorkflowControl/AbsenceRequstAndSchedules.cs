using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequstAndSchedules : IAbsenceRequestAndSchedules
	{
		public AbsenceRequstAndSchedules(IAbsenceRequest absenceRequest, ISchedulingResultStateHolder schedulingResultStateHolder, BudgetGroupState budgetGroupState)
		{
			AbsenceRequest = absenceRequest;
			SchedulingResultStateHolder = schedulingResultStateHolder;
			BudgetGroupState = budgetGroupState;
		}
		
		public IAbsenceRequest AbsenceRequest { get;  }
		public ISchedulingResultStateHolder SchedulingResultStateHolder { get;  }
		public BudgetGroupState BudgetGroupState { get; }
	}
}