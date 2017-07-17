using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestStartTimeValidator : IOvertimeRequestValidator
	{
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private const int minimumApprovalThresholdTimeInMinutes = 15;

		public OvertimeRequestStartTimeValidator(INow now, ILoggedOnUser loggedOnUser)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public OvertimeRequestValidationResult Validate(IPersonRequest personRequest)
		{
			var span = personRequest.Request.Period.StartDateTime - _now.UtcDateTime();
			if (Math.Ceiling(span.TotalMinutes) >= minimumApprovalThresholdTimeInMinutes)
				return new OvertimeRequestValidationResult { IsValid = true };

			return new OvertimeRequestValidationResult
			{
				IsValid = false,
				InvalidReason =
					string.Format(Resources.OvertimeRequestDenyReasonExpired,
						TimeZoneHelper.ConvertFromUtc(personRequest.Request.Period.StartDateTime,
							_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()), minimumApprovalThresholdTimeInMinutes)
			};
		}
	}
}