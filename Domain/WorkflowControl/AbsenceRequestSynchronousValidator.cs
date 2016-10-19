﻿using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestSynchronousValidator : IAbsenceRequestSynchronousValidator
	{
		private readonly IExpiredRequestValidator _expiredRequestValidator;
		private readonly IAlreadyAbsentValidator _alreadyAbsentValidator;
		private readonly IAbsenceRequestWorkflowControlSetValidator _absenceRequestWorkflowControlSetValidator;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public AbsenceRequestSynchronousValidator(IExpiredRequestValidator expiredRequestValidator
			, IAlreadyAbsentValidator alreadyAbsentValidator, IScheduleStorage scheduleStorage
			, ICurrentScenario currentScenario, IAbsenceRequestWorkflowControlSetValidator absenceRequestWorkflowControlSetValidator)
		{
			_expiredRequestValidator = expiredRequestValidator;
			_alreadyAbsentValidator = alreadyAbsentValidator;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_absenceRequestWorkflowControlSetValidator = absenceRequestWorkflowControlSetValidator;
		}

		public IValidatedRequest Validate(IPersonRequest personRequest)
		{
			var result = _absenceRequestWorkflowControlSetValidator.Validate(personRequest);
			if (!result.IsValid)
				return result;

			var person = personRequest.Person;
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), absenceRequest.Period, _currentScenario.Current());

			var scheduleRange = scheduleDictionary[person];
			var requestExpired = _expiredRequestValidator.ValidateExpiredRequest(absenceRequest, scheduleRange);
			if (!requestExpired.IsValid)
				return requestExpired;

			if (_alreadyAbsentValidator.Validate(absenceRequest, scheduleRange))
				return new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestDenyReasonAlreadyAbsent };

			return new ValidatedRequest { IsValid = true };
		}
	}

	public class AbsenceRequestSynchronousValidator40747ToggleOff : IAbsenceRequestSynchronousValidator
	{
		public IValidatedRequest Validate(IPersonRequest personRequest)
		{
			return new ValidatedRequest { IsValid = true };
		}
	}
}