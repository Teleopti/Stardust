using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeScheduleSiteOpenHourFilter : IShiftTradeScheduleSiteOpenHourFilter
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IToggleManager _toggleManager;

		public ShiftTradeScheduleSiteOpenHourFilter(ILoggedOnUser loggedOnUser, IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_toggleManager = toggleManager;
		}

		public IEnumerable<ShiftTradeAddPersonScheduleViewModel> Filter(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> shiftTradeAddPersonScheduleViews, DatePersons datePersons)
		{
			if (!_toggleManager.IsEnabled(Toggles.Wfm_Requests_Site_Open_Hours_39936))
			{
				return shiftTradeAddPersonScheduleViews;
			}

			var shiftTradeDate = datePersons.Date;
			var otherAgentDictionary = datePersons.Persons.ToDictionary(p => p.Id.GetValueOrDefault(Guid.NewGuid()));
			var currentUser = _loggedOnUser.CurrentUser();
			var currentUserSiteOpenHourPeriod = currentUser.SiteOpenHourPeriod(shiftTradeDate);
			if (!currentUserSiteOpenHourPeriod.HasValue)
			{
				currentUserSiteOpenHourPeriod = new TimePeriod(0, 0, 23, 59);
			}

			return shiftTradeAddPersonScheduleViews.Where(
				shiftTradeAddPersonScheduleView =>
				{
					IPerson otherAgent;
					if (!otherAgentDictionary.TryGetValue(shiftTradeAddPersonScheduleView.PersonId, out otherAgent))
					{
						return true;
					}

					var otherAgentSiteOpenHourPeriod = otherAgent.SiteOpenHourPeriod(shiftTradeDate);
					if (!otherAgentSiteOpenHourPeriod.HasValue)
					{
						otherAgentSiteOpenHourPeriod = new TimePeriod(0, 0, 23, 59);
					}

					if (shiftTradeAddPersonScheduleView.ScheduleLayers == null)
					{
						return true;
					}

					var maxEndTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Max(scheduleLayer => scheduleLayer.End);
					var minStartTime = shiftTradeAddPersonScheduleView.ScheduleLayers.Min(scheduleLayer => scheduleLayer.Start);
					var scheduleTimePeriod = new TimePeriod(minStartTime.TimeOfDay, maxEndTime.TimeOfDay);

					return
						currentUserSiteOpenHourPeriod.Value.Contains(scheduleTimePeriod)
						&& otherAgentSiteOpenHourPeriod.Value.Contains(scheduleTimePeriod);
				});
		}
	}
}