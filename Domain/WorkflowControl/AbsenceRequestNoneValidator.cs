using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestNoneValidator : IAbsenceRequestValidator
	{
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string DisplayText
		{
			get { return UserTexts.Resources.No; }
		}

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			return new ValidatedRequest
			{
				IsValid = true,
				ValidationErrors = ""
			};
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new AbsenceRequestNoneValidator();
		}

		public override bool Equals(object obj)
		{
			return obj is AbsenceRequestNoneValidator;
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