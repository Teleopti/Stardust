using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class WriteProtectedScheduleCommandValidator : IWriteProtectedScheduleCommandValidator
	{
		private readonly IAuthorization _authorization;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserCulture _userCulture;

		public WriteProtectedScheduleCommandValidator(IAuthorization authorization,
			ICommonAgentNameProvider commonAgentNameProvider,
			ILoggedOnUser loggedOnUser, IUserCulture userCulture)
		{
			_authorization = authorization;
			_commonAgentNameProvider = commonAgentNameProvider;
			_loggedOnUser = loggedOnUser;
			_userCulture = userCulture;
		}

		public bool ValidateCommand(DateTime date, IPerson agent, IErrorAttachedCommand command, out string errorMessage)
		{
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = _userCulture.GetCulture();

			var scheduleDate = TimeZoneHelper.ConvertFromUtc(date, timeZone);
			if (agentScheduleIsWriteProtected(new DateOnly(scheduleDate), agent))
			{
				errorMessage = createWriteProtectedScheduleErrorMessage(agent, scheduleDate, culture);
				return false;
			}

			errorMessage = null;
			return true;
		}

		private bool agentScheduleIsWriteProtected(DateOnly date, IPerson agent)
		{
			return !_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				   && agent.PersonWriteProtection.IsWriteProtected(date);
		}

		private string createWriteProtectedScheduleErrorMessage(IPerson person, DateTime date, CultureInfo cultureInfo)
		{
			return string.Format(Resources.ScheduleIsWriteProtected,
				_commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
				date.ToString(cultureInfo.DateTimeFormat.ShortDatePattern, cultureInfo));
		}
	}
}