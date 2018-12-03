using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestStaffingAvailablePeriodValidator : IOvertimeRequestValidator
	{
		private readonly INow _now;

		public OvertimeRequestStaffingAvailablePeriodValidator(INow now)
		{
			_now = now;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var personRequest = context.PersonRequest;
			var requestPeriod = personRequest.Request.Period;
			var timezone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone));
			var availablePeriod = new DateOnlyPeriod(today, today.AddDays(context.StaffingDataAvailableDays));
			if (availablePeriod.Contains(requestPeriod.ToDateOnlyPeriod(timezone)))
				return new OvertimeRequestValidationResult {IsValid = true};
			return new OvertimeRequestValidationResult
			{
				IsValid = false,
				InvalidReasons = new[]
				{
					string.Format(Resources.OvertimeRequestDenyReasonInvalidPeriod, availablePeriod)
				}
			};
		}
	}
}
