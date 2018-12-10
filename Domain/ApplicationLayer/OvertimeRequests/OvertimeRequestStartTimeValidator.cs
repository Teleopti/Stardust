using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestStartTimeValidator : IOvertimeRequestValidator
	{
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;

		public OvertimeRequestStartTimeValidator(INow now, ILoggedOnUser loggedOnUser)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var personRequest = context.PersonRequest;
			var span = personRequest.Request.Period.StartDateTime - _now.UtcDateTime();
			if (Math.Ceiling(span.TotalMinutes) >= OvertimeMinimumApprovalThresholdInMinutes.MinimumApprovalThresholdTimeInMinutes)
				return new OvertimeRequestValidationResult { IsValid = true };

			return new OvertimeRequestValidationResult
			{
				IsValid = false,
				InvalidReasons = new []{string.Format(Resources.OvertimeRequestDenyReasonExpired,
					TimeZoneHelper.ConvertFromUtc(personRequest.Request.Period.StartDateTime,
						_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()), OvertimeMinimumApprovalThresholdInMinutes.MinimumApprovalThresholdTimeInMinutes)}
			};
		}
	}
}