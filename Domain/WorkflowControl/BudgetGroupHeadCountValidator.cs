using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class BudgetGroupHeadCountValidator : IAbsenceRequestValidator
	{
		public string InvalidReason => nameof(Resources.RequestDenyReasonBudgetGroupAllowance); //TODO: add text for HeadeCount instead
		public string DisplayText => Resources.BudgetGroupHeadCount;


		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var validatedRequest =
				requiredForHandlingAbsenceRequest.BudgetGroupHeadCountSpecification.IsSatisfied(
					new AbsenceRequstAndSchedules
					{
						AbsenceRequest = absenceRequest,
						SchedulingResultStateHolder = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder
					});
			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new BudgetGroupHeadCountValidator();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as BudgetGroupHeadCountValidator;
			return validator != null;
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
