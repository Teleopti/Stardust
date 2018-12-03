using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestSiteOpenHourValidator : IOvertimeRequestValidator
	{
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;

		public OvertimeRequestSiteOpenHourValidator(ISiteOpenHoursSpecification siteOpenHoursSpecification)
		{
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var personRequest = context.PersonRequest;
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
					new DateOnly(personRequest.Request.Period.StartDateTimeLocal(timeZone)));

			if (siteOpenHour.IsClosed)
			{
				return new OvertimeRequestValidationResult
				{
					InvalidReasons = new []{string.Format(Resources.OvertimeRequestDenyReasonSiteOpenHourClosed,
						requestPeriod,
						siteOpenHour.TimePeriod)}
				};

			}

			return new OvertimeRequestValidationResult
			{
				InvalidReasons = new[]{ string.Format(Resources.OvertimeRequestDenyReasonOutOfSiteOpenHour,
					requestPeriod,
					siteOpenHour.TimePeriod)}
			};
		}
	}
}