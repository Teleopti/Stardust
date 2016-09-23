using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

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

		public override string DenyReason
		{
			get { return "ShiftTradeMaxSeatViolationDenyReason"; }
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{

			if (shiftTradeSwapDetails == null)
				throw new ArgumentNullException("shiftTradeSwapDetails");

			if (!shiftTradeSwapDetails.Any())
			{
				return true;
			}

			_shiftTradeSettings = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings());
			if (!_shiftTradeSettings.MaxSeatsValidationEnabled)
			{
				return true;
			}

			return validateShiftTradeRequestByMaxSeats(shiftTradeSwapDetails.ToList());

		}

		private bool validateShiftTradeRequestByMaxSeats(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{
			return shiftTradeSwapDetails.All(validateShiftTradeSwapDetail);
		}

		private bool validateShiftTradeSwapDetail(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			var personToTeam = shiftTradeSwapDetail.PersonTo.MyTeam(shiftTradeSwapDetail.DateTo);
			var personFromTeam = shiftTradeSwapDetail.PersonFrom.MyTeam(shiftTradeSwapDetail.DateFrom);

			var timeZoneTo = shiftTradeSwapDetail.PersonTo.PermissionInformation.DefaultTimeZone();
			var timeZoneFrom = shiftTradeSwapDetail.PersonFrom.PermissionInformation.DefaultTimeZone();

			if (!needToValidateBasedOnTeamAndSite(personToTeam, personFromTeam))
			{
				return true;
			}

			if (hasMaxSeatViolation(personToTeam.Site,
									 timeZoneTo, shiftTradeSwapDetail.SchedulePartFrom, shiftTradeSwapDetail.SchedulePartTo))
			{
				return false;
			}

			return !hasMaxSeatViolation(personFromTeam.Site,
											timeZoneFrom, shiftTradeSwapDetail.SchedulePartTo, shiftTradeSwapDetail.SchedulePartFrom);
		}

		private static bool needToValidateBasedOnTeamAndSite(ITeam personToTeam, ITeam personFromTeam)
		{
			if (personToTeam == null || personFromTeam == null)
			{
				return false;
			}

			if (personToTeam.Site == null || personFromTeam.Site == null)
			{
				return false;
			}

			if (personToTeam.Site == personFromTeam.Site)
			{
				// if trading within the same site, we dont have a problem as the seats are already allocated.
				return false;
			}

			return true;
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
			if (!seatUsageOnEachIntervalDic.Any())
			{
				return false;
			}


			return _shiftTradeMaxSeatValidator.Validate(site, scheduleDayIncoming, scheduleDayOutgoing, incomingActivitiesRequiringSeat, seatUsageOnEachIntervalDic, timeZoneInfo);
		}

		private IList<ISeatUsageForInterval> createTimeSlotsToCountSeatUsage(IEnumerable<IVisualLayer> incomingActivitiesRequiringSeats)
		{


			var intervalTimespan = TimeSpan.FromMinutes(_shiftTradeSettings.MaxSeatsValidationSegmentLength);
			var seatUsageOnEachIntervalDic = new List<ISeatUsageForInterval>();

			foreach (var activity in incomingActivitiesRequiringSeats)
			{
				// create intervals to check for max seat usage...
				var startInterval = activity.Period.StartDateTime.ToInterval(intervalTimespan.Minutes, IntervalRounding.Down);
				var endInterval = activity.Period.EndDateTime.ToInterval(intervalTimespan.Minutes, IntervalRounding.Up);

				var activityIntervals = new DateTimePeriod(startInterval, endInterval).Intervals(intervalTimespan);

				activityIntervals.ForEach(interval =>
				{
					seatUsageOnEachIntervalDic.Add(new SeatUsageForInterval() { IntervalStart = interval.StartDateTime, IntervalEnd = interval.EndDateTime, SeatUsage = 0 });
				});
			}

			return seatUsageOnEachIntervalDic;
		}

		private static IEnumerable<IVisualLayer> getActivitiesRequiringSeat(IScheduleDay scheduleDay)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projection = personAssignment?.ProjectionService().CreateProjection();
			var activitiesRequiringSeat = projection?.Where(layer =>
		   {
			   var activityLayer = layer.Payload as IActivity;
			   return activityLayer != null && activityLayer.RequiresSeat;
		   });
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