using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestSiteOpenHourValidator : IOvertimeRequestValidator
	{
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;

		public OvertimeRequestSiteOpenHourValidator(ISiteOpenHoursSpecification siteOpenHoursSpecification)
		{
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
		}

		public OvertimeRequestValidationResult Validate(IPersonRequest personRequest)
		{
			var result =
				_siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
				{
					Period = personRequest.Request.Period,
					Person = personRequest.Person
				});

			if (result) return new OvertimeRequestValidationResult { IsValid = true };

			var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var requestPeriod = personRequest.Request.Period.StartDateTimeLocal(timeZone) + " - " +
								personRequest.Request.Period.EndDateTimeLocal(timeZone);
			var siteOpenHour = personRequest.Person.SiteOpenHour(
					new DateOnly(personRequest.Request.Period.StartDateTimeLocal(timeZone))).TimePeriod;

			return new OvertimeRequestValidationResult
			{
				InvalidReason = string.Format(Resources.OvertimeRequestDenyReasonOutOfSiteOpenHour,
					requestPeriod,
					siteOpenHour),
				IsValid = false
			};
		}
	}
}