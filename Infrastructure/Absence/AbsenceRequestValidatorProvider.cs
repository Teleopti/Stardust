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

			if (!_toggleManager.IsEnabled(Toggles.AbsenceRequests_ValidateAllAgentSkills_42392))
			{
				var staffingThresholdValidator = validators.FirstOrDefault(x => x.GetType() == typeof(StaffingThresholdValidator));
				if (staffingThresholdValidator != null)
				{	
					var index = validators.IndexOf(staffingThresholdValidator);

					if (staffingThresholdValidator.GetType() == typeof(StaffingThresholdWithShrinkageValidator))
					{
						validators[index] = new StaffingThresholdValidatorCascadingSkillsWithShrinkage();
					}
					else
					{
						validators[index] = new StaffingThresholdValidatorCascadingSkills();
					}
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

		public IEnumerable<IAbsenceRequestValidator> GetValidatorList(RequestValidatorsFlag validator)
		{
			if (validator.HasFlag(RequestValidatorsFlag.BudgetAllotmentValidator))
				yield return new BudgetGroupHeadCountValidator();
			if (validator.HasFlag(RequestValidatorsFlag.IntradayValidator))
				yield return new StaffingThresholdValidator();
			if (validator.HasFlag(RequestValidatorsFlag.ExpirationValidator))
				yield return new RequestExpirationValidator(_expiredRequestValidator);
		}
	}
}
