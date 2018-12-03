using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class CancelAbsenceRequestCommandHandler : IHandleCommand<CancelAbsenceRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private readonly ICancelAbsenceRequestCommandValidator _cancelAbsenceRequestCommandValidator;
		private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		public CancelAbsenceRequestCommandHandler(IPersonRequestRepository personRequestRepository,
			IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator,
			ICancelAbsenceRequestCommandValidator cancelAbsenceRequestCommandValidator, IGlobalSettingDataRepository globalSettingsDataRepository, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_personRequestRepository = personRequestRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
			_cancelAbsenceRequestCommandValidator = cancelAbsenceRequestCommandValidator;
			_globalSettingsDataRepository = globalSettingsDataRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
		}

		public void Handle(CancelAbsenceRequestCommand command)
		{
			command.ErrorMessages = new List<string>();

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest == null)
			{
				return;
			}

			if (!_writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate, personRequest.Person, command))
			{
				return;
			}

			if (!cancelRequest(personRequest, command)) return;

			command.AffectedRequestId = command.PersonRequestId;
			command.IsReplySuccess = command.TryReplyMessage(personRequest);
		}

		private bool cancelRequest(IPersonRequest personRequest, CancelAbsenceRequestCommand command)
		{
			if (!(personRequest.Request is IAbsenceRequest absenceRequest))
			{
				command.ErrorMessages.Add(Resources.OnlyAbsenceRequestCanBeCancelled);
				return false;
			}

			var person = personRequest.Person;
			var period = personRequest.Request.Period;
			var timezone = personRequest.Request.Person.PermissionInformation.DefaultTimeZone();
			var periodStartDate = new DateOnly(period.StartDateTimeLocal(timezone));
			var periodEndDate = new DateOnly(period.EndDateTimeLocal(timezone));

			var dayScheduleForAbsenceReqStart = getScheduleDay(person,periodStartDate);
			var dayScheduleForAbsenceReqEnd = getScheduleDay(person,periodEndDate);

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(
				period, person,dayScheduleForAbsenceReqStart,dayScheduleForAbsenceReqEnd,_globalSettingsDataRepository);
						
			var personAbsences = _personAbsenceRepository.FindExact(person, adjustedPeriod, absenceRequest.Absence, _currentScenario.Current());

			if (!_cancelAbsenceRequestCommandValidator.ValidateCommand(personRequest, command, absenceRequest, personAbsences))
			{
				return false;
			}

			try
			{				
				var startDate = personAbsences.Min(pa => pa.Period.StartDateTime).AddDays(-1);
				var endDate = personAbsences.Max(pa => pa.Period.EndDateTime).AddDays(1);
				var scheduleRange = getScheduleRange(person, startDate, endDate);
				scheduleRange.CanSeeUnpublishedSchedules = true;

				foreach (var personAbsence in personAbsences)
				{
					var startDateOnly = new DateOnly(personAbsence.Period.StartDateTimeLocal(personRequest.Person.PermissionInformation.DefaultTimeZone()));
					var errorMessages = _personAbsenceRemover.RemovePersonAbsence(startDateOnly,
						personRequest.Person, personAbsence, scheduleRange).ToList();

					if (!errorMessages.Any()) continue;

					command.ErrorMessages = command.ErrorMessages.Concat(errorMessages).ToList();
					return false;
				}

				personRequest.Cancel(_personRequestCheckAuthorization);

				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				command.ErrorMessages.Add(string.Format(Resources.RequestInvalidStateTransition, personRequest.StatusText, Resources.Cancelled));
			}

			return false;
		}


		private IScheduleDay getScheduleDay(IPerson person, DateOnly date)
		{
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new [] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
				_currentScenario.Current());

			return dictionary[person].ScheduledDay(date, true);
		}

		private IScheduleRange getScheduleRange(IPerson person, DateTime startDate, DateTime endDate)
		{
			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					new DateTimePeriod(startDate, endDate),
					_currentScenario.Current());

			return scheduleDictionary[person];
		}
	}
}
