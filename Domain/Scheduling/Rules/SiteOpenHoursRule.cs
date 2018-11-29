using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class SiteOpenHoursRule : INewBusinessRule
	{
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;

		public SiteOpenHoursRule(ISiteOpenHoursSpecification siteOpenHoursSpecification)
		{
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => false;

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

		public string Description => Resources.DescriptionOfSiteOpenHoursRule;

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
			var checkItem = new SiteOpenHoursCheckItem { Period = period, Person = person };
			if (_siteOpenHoursSpecification.IsSatisfiedBy(checkItem)) return null;

			var response = createResponse(person, date, getErrorMessage(person, date, period));
			oldResponses.Add(response);
			return response;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly scheduleDate, string message)
		{
			var friendlyName = Resources.BusinessRuleNoSiteOpenHourFriendlyName;
			var dop = new DateOnlyPeriod(scheduleDate, scheduleDate);
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(typeof(SiteOpenHoursRule), message, HaltModify, IsMandatory,
				period, person, dop, friendlyName)
			{Overridden = !HaltModify };
			return response;
		}

		private string getErrorMessage(IPerson person, DateOnly scheduleDate, DateTimePeriod scheduleTimePeriod)
		{
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dayOfWeek = Resources.ResourceManager.GetString(scheduleDate.DayOfWeek.ToString());
			var siteName = person.MyTeam(scheduleDate).Site.Description.Name;
			return string.Format(currentUiCulture,
				Resources.BusinessRuleNoSiteOpenHourErrorMessage,
				dayOfWeek,
				siteName,
				scheduleTimePeriod.StartDateTimeLocal(timeZone),
				scheduleTimePeriod.EndDateTimeLocal(timeZone));
		}
		
		// NOTE: These strings are only added to create references to translation keys
		private const string monday = nameof(Resources.Monday);
		private const string tuesday = nameof(Resources.Tuesday);
		private const string wednesday = nameof(Resources.Wednesday);
		private const string thursday = nameof(Resources.Thursday);
		private const string friday = nameof(Resources.Friday);
		private const string saturday = nameof(Resources.Saturday);
		private const string sunday = nameof(Resources.Sunday);
	}
}