using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeSiteOpenHourFilter : IShiftTradeSiteOpenHourFilter
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;
		private readonly IProjectionProvider _projectionProvider;

		public ShiftTradeSiteOpenHourFilter(ILoggedOnUser loggedOnUser,
			ISiteOpenHoursSpecification siteOpenHoursSpecification, IProjectionProvider projectionProvider)
		{
			_loggedOnUser = loggedOnUser;
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
			_projectionProvider = projectionProvider;
		}

		public bool FilterSchedule(IScheduleDay toScheduleDay, ShiftTradeAddPersonScheduleViewModel personFromScheduleView)
		{
			var isSatisfiedPersonFromSiteOpenHours = true;
			var isSatisfiedPersonToSiteOpenHours = true;

			if (toScheduleDay == null) return true;
			var personTo = toScheduleDay.Person;
			var personFrom = _loggedOnUser.CurrentUser();
			var projection = _projectionProvider.Projection(toScheduleDay);
			if (projection.HasLayers)
			{
				var personToScheduleTimePeriod = projection.Period().GetValueOrDefault();
				isSatisfiedPersonFromSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
				{
					Period = personToScheduleTimePeriod,
					Person = personFrom
				});
			}
			if (personFromScheduleView.ScheduleLayers != null && personFromScheduleView.ScheduleLayers.Any() && toScheduleDay.Person != null)
			{
				var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView, personFrom.PermissionInformation.DefaultTimeZone());
				isSatisfiedPersonToSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
				{
					Period = personFromSchedulePeriod,
					Person = personTo
				});
			}
			return isSatisfiedPersonToSiteOpenHours && isSatisfiedPersonFromSiteOpenHours;
		}

		public IEnumerable<ShiftTradeAddPersonScheduleViewModel> FilterScheduleView(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> personToScheduleViews,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView, DatePersons datePersons)
		{
			if (personFromScheduleView == null || personFromScheduleView.ScheduleLayers == null || !personFromScheduleView.ScheduleLayers.Any())
			{
				return personToScheduleViews;
			}

			var personDictionary = datePersons.Persons.ToDictionary(p => p.Id.GetValueOrDefault(Guid.NewGuid()));
			var personFrom = _loggedOnUser.CurrentUser();

			var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView, personFrom.PermissionInformation.DefaultTimeZone());

			return personToScheduleViews.Where(
				shiftTradeAddPersonScheduleView =>
				{
					IPerson personTo;
					if (!personDictionary.TryGetValue(shiftTradeAddPersonScheduleView.PersonId, out personTo))
					{
						return true;
					}

					if (shiftTradeAddPersonScheduleView.IsDayOff ||
						shiftTradeAddPersonScheduleView.ScheduleLayers == null || !shiftTradeAddPersonScheduleView.ScheduleLayers.Any())
					{
						return true;
					}

					var personToScheduleTimePeriod = getSchedulePeriod(shiftTradeAddPersonScheduleView, personTo.PermissionInformation.DefaultTimeZone());
					var isSatisfiedPersonFromSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
					{
						Period = personToScheduleTimePeriod,
						Person = personFrom
					});
					var isSatisfiedPersonToSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
					{
						Period = personFromSchedulePeriod,
						Person = personTo
					});

					return isSatisfiedPersonFromSiteOpenHours && isSatisfiedPersonToSiteOpenHours;
				});
		}

		public bool FilterShiftExchangeOffer(IShiftExchangeOffer shiftExchangeOffer,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView)
		{
			if (personFromScheduleView.ScheduleLayers == null || !personFromScheduleView.ScheduleLayers.Any())
			{
				return true;
			}

			var personFrom = _loggedOnUser.CurrentUser();
			var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView, personFrom.PermissionInformation.DefaultTimeZone());

			if (!shiftExchangeOffer.MyShiftPeriod.HasValue)
			{
				return true;
			}

			var personTo = shiftExchangeOffer.Person;
			var personToScheduleDateTimePeriod = shiftExchangeOffer.MyShiftPeriod.Value;
			var isSatisfiedPersonFromSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
			{
				Period = personToScheduleDateTimePeriod,
				Person = personFrom
			});
			var isSatisfiedPersonToSiteOpenHours = _siteOpenHoursSpecification.IsSatisfiedBy(new SiteOpenHoursCheckItem
			{
				Period = personFromSchedulePeriod,
				Person = personTo
			});

			return isSatisfiedPersonFromSiteOpenHours && isSatisfiedPersonToSiteOpenHours;
		}

		private DateTimePeriod getSchedulePeriod(
			ShiftTradeAddPersonScheduleViewModel shiftTradeAddPersonScheduleView, TimeZoneInfo timeZone)
		{
			var maxEndTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Max(scheduleLayer => scheduleLayer.End);
			var minStartTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Min(scheduleLayer => scheduleLayer.Start);
			return new DateTimePeriod(TimeZoneHelper.ConvertToUtc(minStartTime, timeZone), TimeZoneHelper.ConvertToUtc(maxEndTime, timeZone));
		}
	}
}