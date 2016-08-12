using System;
using System.Collections.Generic;
using System.Linq;
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

		public ShiftTradeSiteOpenHourFilter(ILoggedOnUser loggedOnUser, IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_toggleManager = toggleManager;
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

			var shiftTradeDate = datePersons.Date;
			var personDictionary = datePersons.Persons.ToDictionary(p => p.Id.GetValueOrDefault(Guid.NewGuid()));
			var personFromSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(_loggedOnUser.CurrentUser(), shiftTradeDate);
			var personFromSchedulePeriod = getSchedulePeriod(personFromScheduleView);

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

					var personToSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(personTo, shiftTradeDate);
					var personToScheduleTimePeriod = getSchedulePeriod(shiftTradeAddPersonScheduleView);

					return personFromSiteOpenHourPeriod.Contains(personToScheduleTimePeriod)
						   && personToSiteOpenHourPeriod.Contains(personFromSchedulePeriod);
				});
		}

		public IEnumerable<IShiftExchangeOffer> FilterShiftExchangeOffer(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers,
			TimePeriod personFromSchedulePeriod, DateOnly shiftTradeDate)
		{
			if (!isFilterEnabled())
			{
				return shiftExchangeOffers;
			}

			var personFromSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(_loggedOnUser.CurrentUser(), shiftTradeDate);

			return shiftExchangeOffers.Where(
				shiftExchangeOffer =>
				{
					if (!shiftExchangeOffer.MyShiftPeriod.HasValue)
					{
						return true;
					}

					var personToSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(shiftExchangeOffer.Person, shiftTradeDate);

					var timezone = shiftExchangeOffer.Person.PermissionInformation.DefaultTimeZone();
					var personToScheduleTimePeriod = shiftExchangeOffer.MyShiftPeriod.Value.TimePeriod(timezone);

					return
						personFromSiteOpenHourPeriod.Contains(personToScheduleTimePeriod)
						&& personToSiteOpenHourPeriod.Contains(personFromSchedulePeriod);
				});
		}

		private static TimePeriod getPersonSiteOpenHourPeriod(IPerson person, DateOnly shiftTradeDate)
		{
			var siteOpenHourPeriod = person.SiteOpenHourPeriod(shiftTradeDate);
			if (!siteOpenHourPeriod.HasValue)
			{
				siteOpenHourPeriod = new TimePeriod(0, 0, 23, 59);
			}
			return siteOpenHourPeriod.Value;
		}

		private bool isFilterEnabled()
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_Requests_Site_Open_Hours_39936);
		}

		private TimePeriod getSchedulePeriod(ShiftTradeAddPersonScheduleViewModel shiftTradeAddPersonScheduleView)
		{
			var maxEndTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Max(scheduleLayer => scheduleLayer.End);
			var minStartTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Min(scheduleLayer => scheduleLayer.Start);
			var scheduleTimePeriod = new TimePeriod(minStartTime.TimeOfDay, maxEndTime.TimeOfDay);
			return scheduleTimePeriod;
		}
	}
}