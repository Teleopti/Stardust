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

		public CancelAbsenceRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization authorization, IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover, ILoggedOnUser loggedOnUser, IUserCulture userCulture, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_personRequestRepository = personRequestRepository;
			_authorization = authorization;
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
			_loggedOnUser = loggedOnUser;
			_userCulture = userCulture;
			_commonAgentNameProvider = commonAgentNameProvider;
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
				var personAbsenceGroups = groupPersonAbsences(personAbsences);
				// cancellation of the request is handled inside PersonAbsenceRemover, as deleting an absence can also cancel the request
				return removePersonAbsence(personAbsenceGroups);
		
			}
			catch (InvalidRequestStateTransitionException)
			{
				return false;
			}

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
				errorMessage = string.Format(culture, Resources.CanOnlyCancelApprovedAbsenceRequest, personName, absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
			}
			else
			{
				errorMessage = string.Format(culture, Resources.CanOnlyCancelApprovedAbsenceRequestMultiDay,
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
				errorMessage = string.Format(culture, Resources.CouldNotCancelRequestNoAbsence, personName, absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
			}
			else
			{
				errorMessage = string.Format(culture, Resources.CouldNotCancelRequestNoAbsenceMultiDay,
					personName,
					absenceRequest.Period.StartDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture),
					absenceRequest.Period.EndDateTimeLocal(timeZone).Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture));

			}
			return errorMessage;
		}

		private static IEnumerable<PersonAbsenceGrouping> groupPersonAbsences(IEnumerable<IPersonAbsence> personAbsences)
		{
			var personAbsenceGroups = personAbsences
				.GroupBy(pa => new PersonAbsenceGroupKey()
				{
					Date = new DateOnly(pa.Period.StartDateTime),
					Person = pa.Person
				})
				.Select(group => new PersonAbsenceGrouping()
				{
					GroupKey = @group.Key,
					PersonAbsences = @group.Select(x => x)
				}
			);
			return personAbsenceGroups;
		}

		private bool removePersonAbsence(IEnumerable<PersonAbsenceGrouping> personAbsencesGrouped)
		{
			foreach (var personAbsenceGroup in personAbsencesGrouped)
			{

				var errorMessages = _personAbsenceRemover.RemovePersonAbsence(
											personAbsenceGroup.GroupKey.Date,
											personAbsenceGroup.GroupKey.Person,
											personAbsenceGroup.PersonAbsences);

				if (errorMessages.Any())
				{
					return false;
				}

			}

			return true;
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
