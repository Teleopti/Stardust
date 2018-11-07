using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class BudgetGroupHeadCountValidator : IAbsenceRequestValidator
	{
		public string DisplayText => Resources.BudgetGroupHeadCount;


		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var validatedRequest =
				requiredForHandlingAbsenceRequest.BudgetGroupHeadCountSpecification.IsSatisfied(
					new AbsenceRequstAndSchedules(absenceRequest, requiredForHandlingAbsenceRequest.SchedulingResultStateHolder, requiredForHandlingAbsenceRequest.BudgetGroupState));
			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new BudgetGroupHeadCountValidator();
		}

		public override bool Equals(object obj)
		{
			return obj is BudgetGroupHeadCountValidator;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				return result;
			}
		}
	}
}
