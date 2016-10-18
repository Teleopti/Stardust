using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class RequestExpirationValidator : IAbsenceRequestValidator
	{
		private readonly IExpiredRequestValidator _expiredRequestValidator;

		public RequestExpirationValidator(IExpiredRequestValidator expiredRequestValidator)
		{
			_expiredRequestValidator = expiredRequestValidator;
		}

		public string InvalidReason { get; }

		public string DisplayText { get; }

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var scheduleRange = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[absenceRequest.Person];

			return _expiredRequestValidator.ValidateExpiredRequest(absenceRequest, scheduleRange);
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as BudgetGroupHeadCountValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			var result = GetType().GetHashCode();
			return result;
		}
	}
}
