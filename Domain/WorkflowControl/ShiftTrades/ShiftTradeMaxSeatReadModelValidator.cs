using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMaxSeatReadModelValidator : IShiftTradeMaxSeatValidator
	{
		private readonly IScheduleProjectionReadOnlyActivityProvider _scheduleProjectionReadOnlyActivityProvider;
		private readonly ICurrentScenario _currentScenario;

		public ShiftTradeMaxSeatReadModelValidator(
			IScheduleProjectionReadOnlyActivityProvider scheduleProjectionReadOnlyActivityProvider,
			ICurrentScenario currentScenario)
		{
			_scheduleProjectionReadOnlyActivityProvider = scheduleProjectionReadOnlyActivityProvider;
			_currentScenario = currentScenario;
		}

		public bool Validate(ISite site, IScheduleDay scheduleDayIncoming, IScheduleDay scheduleDayOutgoing,
			List<IVisualLayer> incomingActivitiesRequiringSeat, IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic,
			TimeZoneInfo timeZoneInfo)
		{
			var incomingSiteActivies = toSiteActivities(incomingActivitiesRequiringSeat);
			var siteActivities = _scheduleProjectionReadOnlyActivityProvider.GetActivitiesBySite(site,
				scheduleDayIncoming.Period,
				_currentScenario.Current(), true);
			siteActivities.AddRange(incomingSiteActivies);
			return personScheduleCausesMaxSeatViolation(site, scheduleDayOutgoing, siteActivities, seatUsageOnEachIntervalDic);
		}

		private IEnumerable<ISiteActivity> toSiteActivities(IEnumerable<IVisualLayer> visualLayers)
		{
			return visualLayers.Select(visualLayer => new SiteActivity
			{
				ActivityId = visualLayer.Payload.Id.GetValueOrDefault(),
				PersonId = visualLayer.Person.Id.GetValueOrDefault(),
				SiteId = visualLayer.Person.MyTeam(new DateOnly(visualLayer.Period.StartDateTime)).Site.Id.GetValueOrDefault(),
				StartDateTime = visualLayer.Period.StartDateTime,
				EndDateTime = visualLayer.Period.EndDateTime,
				RequiresSeat = true
			});
		}

		private static bool personScheduleCausesMaxSeatViolation(ISite site, IScheduleDay scheduleDayOutgoing,
			IEnumerable<ISiteActivity> siteActivities
			, IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic)
		{
			siteActivities =
				siteActivities.Where(siteActivity => siteActivity.PersonId != scheduleDayOutgoing.Person.Id.GetValueOrDefault());
			return activitiesOnDayAlreadyMatchOrExceedMaximumSeats(site, siteActivities, seatUsageOnEachIntervalDic);
		}


		private static bool activitiesOnDayAlreadyMatchOrExceedMaximumSeats(ISite site,
			IEnumerable<ISiteActivity> siteActivities, IEnumerable<ISeatUsageForInterval> seatUsageOnEachIntervalDic)
		{
			var intervalsThatContainActivity =
				from activity in siteActivities
				from interval in
					seatUsageOnEachIntervalDic.Where(interval => activity.StartDateTime < interval.IntervalEnd &&
																 activity.EndDateTime > interval.IntervalStart)
				select interval;

			foreach (var interval in intervalsThatContainActivity)
			{
				interval.SeatUsage++;
				if (interval.SeatUsage > site.MaxSeats)
				{
					return true;
				}

			}


			return false;
		}

	}
}
