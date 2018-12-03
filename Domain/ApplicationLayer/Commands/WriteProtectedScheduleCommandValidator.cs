using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class WriteProtectedScheduleCommandValidator : IWriteProtectedScheduleCommandValidator
	{
		private readonly IAuthorization _authorization;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserCulture _userCulture;

		public WriteProtectedScheduleCommandValidator(IAuthorization authorization, ICommonAgentNameProvider commonAgentNameProvider, ILoggedOnUser loggedOnUser, IUserCulture userCulture)
		{
			_authorization = authorization;
			_commonAgentNameProvider = commonAgentNameProvider;
			_loggedOnUser = loggedOnUser;
			_userCulture = userCulture;
		}

		public bool ValidateCommand(DateTime date, IPerson agent, IErrorAttachedCommand command)
		{
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = _userCulture.GetCulture();

			var scheduleDate = TimeZoneHelper.ConvertFromUtc(date, timeZone);
			if (agentScheduleIsWriteProtected(new DateOnly(scheduleDate), agent))
			{
				createWriteProtectedScheduleErrorMessage(command, agent, scheduleDate, culture);
				return false;
			}

			return true;
		}

		private bool agentScheduleIsWriteProtected(DateOnly date, IPerson agent)
		{
			return !_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				   && agent.PersonWriteProtection.IsWriteProtected(date);
		}

		private void createWriteProtectedScheduleErrorMessage(IErrorAttachedCommand command, IPerson person, DateTime date, CultureInfo cultureInfo)
		{
			var writeProtectErrorMessage = string.Format(Resources.ScheduleIsWriteProtected,
				_commonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				date.ToString(cultureInfo.DateTimeFormat.ShortDatePattern, cultureInfo));

			command.ErrorMessages = new List<string> { writeProtectErrorMessage };
		}

	}
}