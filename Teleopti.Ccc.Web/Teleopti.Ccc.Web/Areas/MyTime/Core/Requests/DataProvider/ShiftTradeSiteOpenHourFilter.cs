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

			var personDictionary = datePersons.Persons.ToDictionary(p => p.Id.GetValueOrDefault(Guid.NewGuid()));
			var personFrom = _loggedOnUser.CurrentUser();
			var personFromSchedulePeriods = getSchedulePeriods(personFromScheduleView);

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

					var personToScheduleTimePeriods = getSchedulePeriods(shiftTradeAddPersonScheduleView);
					var isSatisfiedPersonFromSiteOpenHours = isSatisfiedSiteOpenHours(personToScheduleTimePeriods, personFrom);
					var isSatisfiedPersonToSiteOpenHours = isSatisfiedSiteOpenHours(personFromSchedulePeriods, personTo);

					return isSatisfiedPersonFromSiteOpenHours && isSatisfiedPersonToSiteOpenHours;
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


		private static bool isSatisfiedSiteOpenHours(Dictionary<DateOnly, TimePeriod> personScheduleTimePeriods, IPerson person)
		{
			var isSatisfied = true;
			foreach (var personToScheduleTimePeriod in personScheduleTimePeriods)
			{
				var personSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(person, personToScheduleTimePeriod.Key);
				isSatisfied = isSatisfied && personSiteOpenHourPeriod.Contains(personToScheduleTimePeriod.Value);
			}
			return isSatisfied;
		}

		private static TimePeriod getPersonSiteOpenHourPeriod(IPerson person, DateOnly shiftTradeDate)
		{
			var siteOpenHour = person.SiteOpenHour(shiftTradeDate);
			if (siteOpenHour == null)
			{
				return new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24).Subtract(new TimeSpan(1)));
			}
			if (siteOpenHour.IsClosed)
			{
				return new TimePeriod();
			}
			return siteOpenHour.TimePeriod;
		}

		private bool isFilterEnabled()
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_Requests_Site_Open_Hours_39936);
		}

		private Dictionary<DateOnly, TimePeriod> getSchedulePeriods(
			ShiftTradeAddPersonScheduleViewModel shiftTradeAddPersonScheduleView)
		{
			var maxEndTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Max(scheduleLayer => scheduleLayer.End);
			var minStartTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Min(scheduleLayer => scheduleLayer.Start);
			return getSchedulePeriods(minStartTime, maxEndTime);
		}

		private Dictionary<DateOnly, TimePeriod> getSchedulePeriods(DateTime startTime, DateTime endTime)
		{
			var dateTimePeriodDictionary = new Dictionary<DateOnly, TimePeriod>();
			if (startTime.Day == endTime.Day)
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, endTime.TimeOfDay));
			}
			else
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, TimeSpan.FromHours(24).Subtract(new TimeSpan(1))));
				dateTimePeriodDictionary.Add(new DateOnly(endTime), new TimePeriod(TimeSpan.Zero, endTime.TimeOfDay));
			}

			return dateTimePeriodDictionary;
		}
	}
}