using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class BudgetGroupAllowanceValidator : IAbsenceRequestValidator
	{
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string DisplayText
		{
			get { return UserTexts.Resources.BudgetGroup; }
		}

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var validatedRequest =
				requiredForHandlingAbsenceRequest.BudgetGroupAllowanceSpecification.IsSatisfied(
					new AbsenceRequstAndSchedules
					{
						AbsenceRequest = absenceRequest,
						SchedulingResultStateHolder = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder
					});

			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new BudgetGroupAllowanceValidator();
		}

		public override bool Equals(object obj)
		{
			return obj is BudgetGroupAllowanceValidator;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
				return result;
			}
		}
	}
}