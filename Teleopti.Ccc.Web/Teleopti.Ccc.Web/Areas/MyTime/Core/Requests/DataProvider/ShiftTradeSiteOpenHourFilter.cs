using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeSiteOpenHourFilter : IShiftTradeSiteOpenHourFilter
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IToggleManager _toggleManager;
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;
		private readonly IProjectionProvider _projectionProvider;

		public ShiftTradeSiteOpenHourFilter(ILoggedOnUser loggedOnUser, IToggleManager toggleManager,
			ISiteOpenHoursSpecification siteOpenHoursSpecification, IProjectionProvider projectionProvider)
		{
			_loggedOnUser = loggedOnUser;
			_toggleManager = toggleManager;
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
			_projectionProvider = projectionProvider;
		}

		public bool FilterSchedule(IScheduleDay toScheduleDay, ShiftTradeAddPersonScheduleViewModel personFromScheduleView)
		{
			if (!isFilterEnabled())
			{
				return true;
			}

			if (toScheduleDay == null)
			{
				return true;
			}

			if (personFromScheduleView.ScheduleLayers == null || !personFromScheduleView.ScheduleLayers.Any())
			{
				return true;
			}

			var personTo = toScheduleDay.Person;
			var personFrom = _loggedOnUser.CurrentUser();

			var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView, personFrom.PermissionInformation.DefaultTimeZone());
			var projection = _projectionProvider.Projection (toScheduleDay);
			if (!projection.HasLayers)
			{
				return true;
			}

			var personToScheduleTimePeriod = projection.Period().GetValueOrDefault();
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

		}

		public IEnumerable<ShiftTradeAddPersonScheduleViewModel> FilterScheduleView(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> personToScheduleViews,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView, DatePersons datePersons)
		{
			if (!isFilterEnabled())
			{
				return personToScheduleViews;
			}

			if (personFromScheduleView.ScheduleLayers == null || !personFromScheduleView.ScheduleLayers.Any())
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

					if (shiftTradeAddPersonScheduleView.ScheduleLayers == null || !shiftTradeAddPersonScheduleView.ScheduleLayers.Any())
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

		public IEnumerable<IShiftExchangeOffer> FilterShiftExchangeOffer(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView)
		{
			if (!isFilterEnabled())
			{
				return shiftExchangeOffers;
			}

			if (personFromScheduleView.ScheduleLayers == null || !personFromScheduleView.ScheduleLayers.Any())
			{
				return shiftExchangeOffers;
			}

			var personFrom = _loggedOnUser.CurrentUser();
			var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView, personFrom.PermissionInformation.DefaultTimeZone());

			return shiftExchangeOffers.Where(
				shiftExchangeOffer =>
				{
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
				});
		}

		private bool isFilterEnabled()
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_Requests_Site_Open_Hours_39936);
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