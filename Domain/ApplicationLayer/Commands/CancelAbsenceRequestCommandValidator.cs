using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class CancelAbsenceRequestCommandValidator : ICancelAbsenceRequestCommandValidator
	{
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserCulture _userCulture;

		public CancelAbsenceRequestCommandValidator(IPersonRequestCheckAuthorization personRequestCheckAuthorization, ICommonAgentNameProvider commonAgentNameProvider, ILoggedOnUser loggedOnUser, IUserCulture userCulture)
		{
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_commonAgentNameProvider = commonAgentNameProvider;
			_loggedOnUser = loggedOnUser;
			_userCulture = userCulture;
		}

		public bool ValidateCommand(IPersonRequest personRequest, CancelAbsenceRequestCommand command, IAbsenceRequest absenceRequest, ICollection<IPersonAbsence> personAbsences)
		{

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = _userCulture.GetCulture();
			
			if (!_personRequestCheckAuthorization.HasCancelRequestPermission(personRequest))
			{
				command.ErrorMessages.Add(Resources.InsufficientPermission);
				return false;
			}

			if (!personRequest.IsApproved)
			{
				command.ErrorMessages.Add(createNotApprovedRequestErrorMessage(absenceRequest, timeZone, culture));
				return false;
			}


			if (personAbsences.Count == 0)
			{
				command.ErrorMessages.Add(createNoAbsenceErrorMessage(absenceRequest, timeZone, culture));
				return false;
			}
			return true;
		}

		private string createNotApprovedRequestErrorMessage (IAbsenceRequest absenceRequest, TimeZoneInfo timeZone, CultureInfo culture)
		{
	
			var personName = _commonAgentNameProvider.CommonAgentNameSettings.BuildFor(absenceRequest.Person);
	
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

			var personName = _commonAgentNameProvider.CommonAgentNameSettings.BuildFor(absenceRequest.Person);

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
}