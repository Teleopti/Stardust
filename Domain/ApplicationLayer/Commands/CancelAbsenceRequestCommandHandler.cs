using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class CancelAbsenceRequestCommandHandler : IHandleCommand<CancelAbsenceRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserCulture _userCulture;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public CancelAbsenceRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization authorization, IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover, ILoggedOnUser loggedOnUser, IUserCulture userCulture, ICommonAgentNameProvider commonAgentNameProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_personRequestRepository = personRequestRepository;
			_authorization = authorization;
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
			_loggedOnUser = loggedOnUser;
			_userCulture = userCulture;
			_commonAgentNameProvider = commonAgentNameProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public void Handle(CancelAbsenceRequestCommand command)
		{

			command.ErrorMessages = new List<string>();

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest != null && cancelRequest(personRequest, command))
			{
				command.AffectedRequestId = command.PersonRequestId;

			}
		}

		private bool cancelRequest(IPersonRequest personRequest, CancelAbsenceRequestCommand command)
		{
			var absenceRequest = personRequest.Request as IAbsenceRequest;

			if (absenceRequest == null)
			{
				return false;
			}

			var personAbsences = _personAbsenceRepository.Find(absenceRequest);

			if (!validateCancelRequestCommand (personRequest, command, absenceRequest, personAbsences))
			{
				return false;
			}

			try
			{
				var person = personRequest.Person;
				var startDate = personAbsences.Min(pa => pa.Period.StartDateTime);
				var endDate = personAbsences.Max (pa => pa.Period.EndDateTime);
				var scheduleRange = getScheduleRange(person, startDate, endDate);

				IList<string> errorMessages = new List<string>();

				foreach (var personAbsence in personAbsences)
				{
					errorMessages = _personAbsenceRemover.RemovePersonAbsence (
						new DateOnly (personAbsence.Period.LocalStartDateTime),
						personRequest.Person,
						new[] {personAbsence},
						scheduleRange).ToList();

					if (errorMessages.Any())
					{
						command.ErrorMessages = command.ErrorMessages.Concat (errorMessages).ToList() ;
						return false;
					}
				}

				return true;
			}

			catch (InvalidRequestStateTransitionException)
			{
				return false;
			}

		}

		private IScheduleRange getScheduleRange (IPerson person, DateTime startDate, DateTime endDate)
		{
			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod (person,
					new ScheduleDictionaryLoadOptions (false, false),
					new DateTimePeriod (startDate, endDate),
					_currentScenario.Current());

			return scheduleDictionary[person];
		}

		private bool validateCancelRequestCommand (IPersonRequest personRequest, CancelAbsenceRequestCommand command, IAbsenceRequest absenceRequest, ICollection<IPersonAbsence> personAbsences)
		{
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = _userCulture.GetCulture();

			if (!_authorization.HasCancelRequestPermission (personRequest))
			{
				command.ErrorMessages.Add (Resources.InsufficientPermission);
				return false;
			}

			if (!personRequest.IsApproved)
			{
				command.ErrorMessages.Add (createNotApprovedRequestErrorMessage (absenceRequest, timeZone, culture));
				return false;
			}


			if (personAbsences.Count == 0)
			{
				command.ErrorMessages.Add (createNoAbsenceErrorMessage (absenceRequest, timeZone, culture));
				return false;
			}
			return true;
		}


		private string createNotApprovedRequestErrorMessage(IAbsenceRequest absenceRequest, TimeZoneInfo timeZone, CultureInfo culture)
		{

			var personName = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(absenceRequest.Person);

			string errorMessage;

			if (absenceRequest.IsRequestForOneLocalDay(timeZone))
			{
				errorMessage = string.Format(Resources.CanOnlyCancelApprovedAbsenceRequest, personName, absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
			}
			else
			{
				errorMessage = string.Format(Resources.CanOnlyCancelApprovedAbsenceRequestMultiDay,
					personName,
					absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture),
					absenceRequest.Period.EndDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture));

			}
			return errorMessage;
		}

		private string createNoAbsenceErrorMessage(IAbsenceRequest absenceRequest, TimeZoneInfo timeZone, CultureInfo culture)
		{

			var personName = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(absenceRequest.Person);

			string errorMessage;

			if (absenceRequest.IsRequestForOneLocalDay(timeZone))
			{
				errorMessage = string.Format(Resources.CouldNotCancelRequestNoAbsence, personName, absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
			}
			else
			{
				errorMessage = string.Format(Resources.CouldNotCancelRequestNoAbsenceMultiDay,
					personName,
					absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture),
					absenceRequest.Period.EndDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture));

			}
			return errorMessage;
		}
	}

	internal class PersonAbsenceGroupKey
	{
		public DateOnly Date { get; set; }
		public IPerson Person { get; set; }

	}
	internal class PersonAbsenceGrouping
	{
		public PersonAbsenceGroupKey GroupKey { get; set; }
		public IEnumerable<IPersonAbsence> PersonAbsences { get; set; }
	}
}
