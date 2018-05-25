using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestSynchronousValidator : IAbsenceRequestSynchronousValidator
	{
		private readonly IExpiredRequestValidator _expiredRequestValidator;
		private readonly IAlreadyAbsentValidator _alreadyAbsentValidator;
		private readonly IAbsenceRequestWorkflowControlSetValidator _absenceRequestWorkflowControlSetValidator;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IAbsenceRequestPersonAccountValidator _absenceRequestPersonAccountValidator;
		private readonly IAnyPersonSkillsOpenValidator _anyPersonSkillsOpenValidator;

		public AbsenceRequestSynchronousValidator(IExpiredRequestValidator expiredRequestValidator
			, IAlreadyAbsentValidator alreadyAbsentValidator, IScheduleStorage scheduleStorage
			, ICurrentScenario currentScenario, IAbsenceRequestWorkflowControlSetValidator absenceRequestWorkflowControlSetValidator, 
			IAbsenceRequestPersonAccountValidator absenceRequestPersonAccountValidator, IAnyPersonSkillsOpenValidator anyPersonSkillsOpenValidator)
		{
			_expiredRequestValidator = expiredRequestValidator;
			_alreadyAbsentValidator = alreadyAbsentValidator;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_absenceRequestWorkflowControlSetValidator = absenceRequestWorkflowControlSetValidator;
			_absenceRequestPersonAccountValidator = absenceRequestPersonAccountValidator;
			_anyPersonSkillsOpenValidator = anyPersonSkillsOpenValidator;
		}

		public IValidatedRequest Validate(IPersonRequest personRequest)
		{
			var result = _absenceRequestWorkflowControlSetValidator.Validate(personRequest);
			if (!result.IsValid)
				return result;

			var person = personRequest.Person;
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), absenceRequest.Period.ToDateOnlyPeriod(
				person.PermissionInformation.DefaultTimeZone()).Inflate(1)
				, _currentScenario.Current());

			var scheduleRange = scheduleDictionary[person];
			var requestExpired = _expiredRequestValidator.ValidateExpiredRequest(absenceRequest, scheduleRange);
			if (!requestExpired.IsValid)
				return requestExpired;

			if (_alreadyAbsentValidator.Validate(absenceRequest, scheduleRange))
				return new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestDenyReasonAlreadyAbsent
					, DenyOption = PersonRequestDenyOption.AlreadyAbsence };

			var personAccountValidateResult = _absenceRequestPersonAccountValidator.Validate(personRequest, scheduleRange);
			if (!personAccountValidateResult.IsValid)
				return new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestDenyReasonPersonAccount
					, DenyOption = PersonRequestDenyOption.InsufficientPersonAccount };

			var personSkills =
				person.PersonPeriods(personRequest.Request.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone())).
					SelectMany(x => x.PersonSkillCollection).Distinct();
			return _anyPersonSkillsOpenValidator.Validate(absenceRequest, personSkills, scheduleRange);
		}

		public IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange, IPersonAbsenceAccount personAbsenceAccount)
		{
			var result = _absenceRequestWorkflowControlSetValidator.Validate(personRequest);
			if (!result.IsValid)
				return result;

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			var requestExpired = _expiredRequestValidator.ValidateExpiredRequest(absenceRequest, scheduleRange);
			if (!requestExpired.IsValid)
				return requestExpired;

			if (_alreadyAbsentValidator.Validate(absenceRequest, scheduleRange))
				return new ValidatedRequest { IsValid = false, ValidationErrors = nameof(Resources.RequestDenyReasonAlreadyAbsent)
					, DenyOption = PersonRequestDenyOption.AlreadyAbsence };

			var workflowControlSet = personRequest.Person.WorkflowControlSet;
			var absenceRequestOpenPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
			if (absenceRequestOpenPeriod?.PersonAccountValidator is AbsenceRequestNoneValidator)
			{
				return ValidatedRequest.Valid;
			}

			var personAccountValidateResult = _absenceRequestPersonAccountValidator.ValidatedRequestWithPersonAccount(personRequest, scheduleRange, personAbsenceAccount);
			if (!personAccountValidateResult.IsValid)
				return new ValidatedRequest { IsValid = false, ValidationErrors = nameof(Resources.RequestDenyReasonPersonAccount)
					, DenyOption = PersonRequestDenyOption.InsufficientPersonAccount };

			// We don't check open hours when we process waitlist 

			return new ValidatedRequest{IsValid = true};
		}
	}
}