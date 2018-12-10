using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMaxSeatsSpecification : ShiftTradeSpecification
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IShiftTradeMaxSeatValidator _shiftTradeMaxSeatValidator;
		private ShiftTradeSettings _shiftTradeSettings;

		public ShiftTradeMaxSeatsSpecification(IGlobalSettingDataRepository globalSettingDataRepository, IShiftTradeMaxSeatValidator shiftTradeMaxSeatValidator)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_shiftTradeMaxSeatValidator = shiftTradeMaxSeatValidator;
		}

		public override string DenyReason => nameof(Resources.ShiftTradeMaxSeatViolationDenyReason);

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{
			if (shiftTradeSwapDetails == null)
				throw new ArgumentNullException(nameof(shiftTradeSwapDetails));

			var swapDetails = shiftTradeSwapDetails.ToArray();
			if (!swapDetails.Any())
			{
				return true;
			}

			_shiftTradeSettings = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings());
			return !_shiftTradeSettings.MaxSeatsValidationEnabled || validateShiftTradeRequestByMaxSeats(swapDetails);
		}

		private bool validateShiftTradeRequestByMaxSeats(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{
			return shiftTradeSwapDetails.All(validateShiftTradeSwapDetail);
		}

		private bool validateShiftTradeSwapDetail(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			var teamTo = shiftTradeSwapDetail.PersonTo.MyTeam(shiftTradeSwapDetail.DateTo);
			var teamFrom = shiftTradeSwapDetail.PersonFrom.MyTeam(shiftTradeSwapDetail.DateFrom);

			var timeZoneTo = shiftTradeSwapDetail.PersonTo.PermissionInformation.DefaultTimeZone();
			var timeZoneFrom = shiftTradeSwapDetail.PersonFrom.PermissionInformation.DefaultTimeZone();

			if (!needToValidateBasedOnTeamAndSite(teamTo, teamFrom))
			{
				return true;
			}

			if (hasMaxSeatViolation(teamTo.Site, timeZoneTo, shiftTradeSwapDetail.SchedulePartFrom,
				shiftTradeSwapDetail.SchedulePartTo))
			{
				return false;
			}

			return
				!hasMaxSeatViolation(teamFrom.Site, timeZoneFrom, shiftTradeSwapDetail.SchedulePartTo,
					shiftTradeSwapDetail.SchedulePartFrom);
		}

		private static bool needToValidateBasedOnTeamAndSite(ITeam personToTeam, ITeam personFromTeam)
		{
			// if trading within the same site, we dont have a problem as the seats are already allocated.
			return personToTeam != null && personFromTeam != null
				   && personToTeam.Site != null && personFromTeam.Site != null
				   && personToTeam.Site != personFromTeam.Site;
		}

		private bool hasMaxSeatViolation(ISite site, TimeZoneInfo timeZoneInfo, IScheduleDay scheduleDayIncoming, IScheduleDay scheduleDayOutgoing)
		{
			if (!site.MaxSeats.HasValue) return false;

			var incomingActivitiesRequiringSeat = getActivitiesRequiringSeat(scheduleDayIncoming)?.ToList();
			if (incomingActivitiesRequiringSeat == null || !incomingActivitiesRequiringSeat.Any())
			{
				return false;
			}

			var seatUsageOnEachIntervalDic = createTimeSlotsToCountSeatUsage(incomingActivitiesRequiringSeat);
			return seatUsageOnEachIntervalDic.Any() &&
				   _shiftTradeMaxSeatValidator.Validate(site, scheduleDayIncoming, scheduleDayOutgoing,
					   incomingActivitiesRequiringSeat, seatUsageOnEachIntervalDic, timeZoneInfo);
		}

		private IList<ISeatUsageForInterval> createTimeSlotsToCountSeatUsage(IEnumerable<IVisualLayer> incomingActivitiesRequiringSeats)
		{
			var intervalTimespan = TimeSpan.FromMinutes(_shiftTradeSettings.MaxSeatsValidationSegmentLength);
			var seatUsageOnEachIntervalDic = new List<ISeatUsageForInterval>();

			foreach (var activity in incomingActivitiesRequiringSeats)
			{
				// create intervals to check for max seat usage...
				var startInterval = activity.Period.StartDateTime.ToInterval((int)intervalTimespan.TotalMinutes, IntervalRounding.Down);
				var endInterval = activity.Period.EndDateTime.ToInterval((int)intervalTimespan.TotalMinutes, IntervalRounding.Up);

				var activityIntervals = new DateTimePeriod(startInterval, endInterval).Intervals(intervalTimespan);

				activityIntervals.ForEach(interval =>
				{
					seatUsageOnEachIntervalDic.Add(new SeatUsageForInterval
					{
						IntervalStart = interval.StartDateTime,
						IntervalEnd = interval.EndDateTime,
						SeatUsage = 0
					});
				});
			}

			return seatUsageOnEachIntervalDic;
		}

		private static IEnumerable<IVisualLayer> getActivitiesRequiringSeat(IScheduleDay scheduleDay)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projection = personAssignment?.ProjectionService().CreateProjection();
			var activitiesRequiringSeat = projection?.Where(layer => layer.Payload is IActivity activityLayer && activityLayer.RequiresSeat);
			return activitiesRequiringSeat;
		}
	}

	public class SeatUsageForInterval : ISeatUsageForInterval
	{
		public DateTime IntervalStart { get; set; }
		public DateTime IntervalEnd { get; set; }
		public int SeatUsage { get; set; }
	}
}