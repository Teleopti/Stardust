using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
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
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> shiftTradeAddPersonScheduleViews, DatePersons datePersons)
		{
			if (!isFilterEnabled())
			{
				return shiftTradeAddPersonScheduleViews;
			}

			var shiftTradeDate = datePersons.Date;
			var otherAgentDictionary = datePersons.Persons.ToDictionary(p => p.Id.GetValueOrDefault(Guid.NewGuid()));
			var currentUserSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(_loggedOnUser.CurrentUser(), shiftTradeDate);

			return shiftTradeAddPersonScheduleViews.Where(
				shiftTradeAddPersonScheduleView =>
				{
					IPerson otherAgent;
					if (!otherAgentDictionary.TryGetValue(shiftTradeAddPersonScheduleView.PersonId, out otherAgent))
					{
						return true;
					}

					if (shiftTradeAddPersonScheduleView.ScheduleLayers == null || !shiftTradeAddPersonScheduleView.ScheduleLayers.Any())
					{
						return true;
					}

					var otherAgentSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(otherAgent, shiftTradeDate);

					var maxEndTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Max(scheduleLayer => scheduleLayer.End);
					var minStartTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Min(scheduleLayer => scheduleLayer.Start);
					var scheduleTimePeriod = new TimePeriod(minStartTime.TimeOfDay, maxEndTime.TimeOfDay);

					return
						currentUserSiteOpenHourPeriod.Contains(scheduleTimePeriod)
						&& otherAgentSiteOpenHourPeriod.Contains(scheduleTimePeriod);
				});
		}

		public IEnumerable<IShiftExchangeOffer> FilterShiftExchangeOffer(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers, DateOnly shiftTradeDate)
		{
			if (!isFilterEnabled())
			{
				return shiftExchangeOffers;
			}

			var currentUserSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(_loggedOnUser.CurrentUser(), shiftTradeDate);

			return shiftExchangeOffers.Where(
				shiftExchangeOffer =>
				{
					if (!shiftExchangeOffer.MyShiftPeriod.HasValue)
					{
						return true;
					}

					var otherAgentSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(shiftExchangeOffer.Person, shiftTradeDate);

					var timezone = shiftExchangeOffer.Person.PermissionInformation.DefaultTimeZone();
					var maxEndTime = shiftExchangeOffer.MyShiftPeriod.Value.EndDateTimeLocal(timezone);
					var minStartTime = shiftExchangeOffer.MyShiftPeriod.Value.StartDateTimeLocal(timezone);
					var scheduleTimePeriod = new TimePeriod(minStartTime.TimeOfDay, maxEndTime.TimeOfDay);

					return
						currentUserSiteOpenHourPeriod.Contains(scheduleTimePeriod)
						&& otherAgentSiteOpenHourPeriod.Contains(scheduleTimePeriod);
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
	}
}