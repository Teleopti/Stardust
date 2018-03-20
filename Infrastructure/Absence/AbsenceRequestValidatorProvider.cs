using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestValidatorProvider : IAbsenceRequestValidatorProvider
	{
		private readonly IExpiredRequestValidator _expiredRequestValidator;

		public AbsenceRequestValidatorProvider(IExpiredRequestValidator expiredRequestValidator)
		{
			_expiredRequestValidator = expiredRequestValidator;
		}

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var validators = absenceRequestOpenPeriod.GetSelectedValidatorList().ToList();

			var requestExpirationValidator = new RequestExpirationValidator(_expiredRequestValidator);
			if (!validators.Contains(requestExpirationValidator))
			{
				validators.Insert(0, requestExpirationValidator);
			}

			return validators;
		}

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(IPersonRequest personRequest, RequestValidatorsFlag validator)
		{
			var absenceRequestValidatorList = new List<IAbsenceRequestValidator>();

			if (validator.HasFlag(RequestValidatorsFlag.BudgetAllotmentValidator))
				absenceRequestValidatorList.Add(new BudgetGroupHeadCountValidator());

			if (validator.HasFlag(RequestValidatorsFlag.IntradayValidator))
			{
				absenceRequestValidatorList.Add(getIntradayValidator(personRequest));
			}

			if (validator.HasFlag(RequestValidatorsFlag.ExpirationValidator))
				absenceRequestValidatorList.Add(new RequestExpirationValidator(_expiredRequestValidator));

			return absenceRequestValidatorList;
		}

		public bool IsAnyStaffingValidatorEnabled(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var validators = GetValidatorList(absenceRequestOpenPeriod).ToList();
			if (validators.Any(v => v is StaffingThresholdValidator) ||
				validators.Any(v => v is StaffingThresholdValidatorCascadingSkillsWithShrinkage) ||
				validators.Any(v => v is BudgetGroupAllowanceValidator) ||
				validators.Any(v => v is BudgetGroupHeadCountValidator) ||
				validators.Any(v => v is StaffingThresholdWithShrinkageValidator))
				return true;
			return false;
		}

		public bool IsValidatorEnabled<T>(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
			where T : IAbsenceRequestValidator
		{
			var validators = GetValidatorList(absenceRequestOpenPeriod).ToList();
			return validators.Any(v => v is T);
		}

		private IAbsenceRequestValidator getIntradayValidator(IPersonRequest personRequest)
		{
			var validators = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod(
				(IAbsenceRequest) personRequest.Request).GetSelectedValidatorList();
			var staffingThresholdValidator =
				validators.FirstOrDefault(x => x is StaffingThresholdValidator);
			if (staffingThresholdValidator == null)
			{
				return new StaffingThresholdValidator();
			}
			return staffingThresholdValidator;
		}
	}
}
