using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class SiteOpenHoursRule : INewBusinessRule
	{
		private bool _haltModify = true;

		public string ErrorMessage
		{
			get { return string.Empty; }
		}

		public bool IsMandatory
		{
			get { return false; }
		}

		public bool HaltModify
		{
			get { return _haltModify; }
			set { _haltModify = value; }
		}

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var responseList = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				var response = checkScheduleDay(rangeClones, scheduleDay);
				if (response != null)
				{
					responseList.Add(response);
				}
			}
			return responseList;
		}

		private IBusinessRuleResponse checkScheduleDay(IDictionary<IPerson, IScheduleRange> rangeClones, IScheduleDay scheduleDay)
		{
			var person = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var currentSchedules = rangeClones[person];
			var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
			oldResponses.Remove(createResponse(person, date, "remove"));

			if (!scheduleDay.HasProjection())
				return null;

			var layerCollection = scheduleDay.ProjectionService().CreateProjection();
			if (layerCollection == null || !layerCollection.HasLayers)
				return null;

			var period = layerCollection.Period().Value;
			var scheduleTimePeriod = period.TimePeriod(person.PermissionInformation.DefaultTimeZone());
			var siteOpenHourPeriod = getPersonSiteOpenHourPeriod(person, date);
			if (!siteOpenHourPeriod.Contains(scheduleTimePeriod))
			{
				var response = createResponse(person, date, getErrorMessage(person, date, period));
				oldResponses.Add(response);
				return response;
			}

			return null;
		}

		private static TimePeriod getPersonSiteOpenHourPeriod(IPerson person, DateOnly scheduleDate)
		{
			var siteOpenHour = person.SiteOpenHour(scheduleDate);
			if (siteOpenHour == null)
			{
				return new TimePeriod(0, 0, 23, 59);
			}
			if (siteOpenHour.IsClosed)
			{
				return new TimePeriod();
			}
			return siteOpenHour.TimePeriod;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly scheduleDate, string message)
		{
			var dop = new DateOnlyPeriod(scheduleDate, scheduleDate);
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(typeof(SiteOpenHoursRule), message, HaltModify, IsMandatory,
				period, person, dop)
			{Overridden = !_haltModify };
			return response;
		}

		private string getErrorMessage(IPerson person, DateOnly scheduleDate, DateTimePeriod scheduleTimePeriod)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dayOfWeek = UserTexts.Resources.ResourceManager.GetString(scheduleDate.DayOfWeek.ToString());
			var siteName = person.MyTeam(scheduleDate).Site.Description.Name;
			return string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
				UserTexts.Resources.BusinessRuleNoSiteOpenHourErrorMessage,
				dayOfWeek,
				siteName,
				scheduleTimePeriod.StartDateTimeLocal(timeZone),
				scheduleTimePeriod.EndDateTimeLocal(timeZone));
		}
	}
}
