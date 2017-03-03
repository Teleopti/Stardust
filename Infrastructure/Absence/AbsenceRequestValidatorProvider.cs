using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;

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
