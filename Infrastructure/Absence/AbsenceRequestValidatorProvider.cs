using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestValidatorProvider : IAbsenceRequestValidatorProvider
	{
		private readonly IToggleManager _toggleManager;
		private readonly IExpiredRequestValidator _expiredRequestValidator;

		public AbsenceRequestValidatorProvider(IToggleManager toggleManager, IExpiredRequestValidator expiredRequestValidator)
		{
			_toggleManager = toggleManager;
			_expiredRequestValidator = expiredRequestValidator;
		}

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var validators = absenceRequestOpenPeriod.GetSelectedValidatorList().ToList();

			if (!validateAllAgentSkills())
			{
				var staffingThresholdValidator = validators.FirstOrDefault(x => x.GetType() == typeof(StaffingThresholdValidator));
				if (staffingThresholdValidator != null)
				{
					var index = validators.IndexOf(staffingThresholdValidator);
					validators[index] = getStaffingThresholdValidatorCascadingSkills(staffingThresholdValidator);
				}
			}

			if (_toggleManager.IsEnabled(Toggles.Wfm_Requests_Check_Expired_Requests_40274))
			{
				var requestExpirationValidator = new RequestExpirationValidator(_expiredRequestValidator);
				if (!validators.Contains(requestExpirationValidator))
					validators.Insert(0, requestExpirationValidator);
			}

			return validators;
		}

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(IPersonRequest personRequest, RequestValidatorsFlag validator)
		{
			if (validator.HasFlag(RequestValidatorsFlag.BudgetAllotmentValidator))
				yield return new BudgetGroupHeadCountValidator();
			if (validator.HasFlag(RequestValidatorsFlag.IntradayValidator))
			{
				yield return getIntradayValidator(personRequest);
			}
			if (validator.HasFlag(RequestValidatorsFlag.ExpirationValidator))
				yield return new RequestExpirationValidator(_expiredRequestValidator);
		}

		private IAbsenceRequestValidator getIntradayValidator(IPersonRequest personRequest)
		{
			var validators = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod(
				(IAbsenceRequest) personRequest.Request).GetSelectedValidatorList();
			var staffingThresholdValidator = validators.FirstOrDefault(x => x.GetType() == typeof(StaffingThresholdValidator));
			if (staffingThresholdValidator == null)
			{
				return new AbsenceRequestNoneValidator();
			}
			if (!validateAllAgentSkills())
			{
				staffingThresholdValidator = getStaffingThresholdValidatorCascadingSkills(staffingThresholdValidator);
			}
			return staffingThresholdValidator;
		}

		private bool validateAllAgentSkills()
		{
			return _toggleManager.IsEnabled(Toggles.AbsenceRequests_ValidateAllAgentSkills_42392);
		}

		private IAbsenceRequestValidator getStaffingThresholdValidatorCascadingSkills(
			IAbsenceRequestValidator staffingThresholdValidator)
		{
			if (staffingThresholdValidator.GetType() == typeof(StaffingThresholdWithShrinkageValidator))
			{
				return new StaffingThresholdValidatorCascadingSkillsWithShrinkage();
			}
			return new StaffingThresholdValidatorCascadingSkills();
		}
	}
}
