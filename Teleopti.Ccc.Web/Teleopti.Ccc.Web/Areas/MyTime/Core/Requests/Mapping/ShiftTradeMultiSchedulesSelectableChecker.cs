using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeMultiSchedulesSelectableChecker
	{
		bool CheckSelectable(bool hasAbsence, IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, DateOnly date, IPerson personTo, out string unSelectableReason);
	}

	public class ShiftTradeMultiSchedulesSelectableChecker : IShiftTradeMultiSchedulesSelectableChecker
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeMultiSchedulesSelectableChecker(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public bool CheckSelectable(bool hasAbsence, IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, 
									DateOnly date, IPerson personTo, out string unSelectableReason)
		{
			unSelectableReason = "";

			if (hasAbsence)
			{
				unSelectableReason = Resources.AbsenceCannotBeTraded;
				return false;
			}

			if (!isDateSatisfied(myScheduleDay, personToScheduleDay, date, personTo))
			{
				unSelectableReason = String.Format(Resources.ScheduleDateDoNotMatch, personTo.Name);
				return false;
			}

			if (!isSkillSatisfied(personTo, date))
			{
				unSelectableReason = String.Format(Resources.SkillDoNotMatch, personTo.Name);
				return false;
			}

			if (isOutsideOpenHours(myScheduleDay, personToScheduleDay, personTo, date))
			{
				unSelectableReason = Resources.NotAllowedWhenShiftOutsideOpenHours;
				return false;
			}

			return true;
		}

		private bool isOutsideOpenHours(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, IPerson personTo, DateOnly date)
		{
			var personToSiteOpenHour = personTo.SiteOpenHour(date);
			var personToOpenStart = personToSiteOpenHour.TimePeriod.StartTime;
			var personToOpenEnd = personToSiteOpenHour.TimePeriod.EndTime;
			var myShiftLayers = myScheduleDay?.PersonAssignment()?.ShiftLayers;
			if (myShiftLayers != null || myShiftLayers.Any())
			{
				var myScheduleStart = myShiftLayers.First().Period.StartDateTime;
				var myScheduleEnd = myShiftLayers.Last().Period.EndDateTime;
				var personToTimezone = personTo.PermissionInformation.DefaultTimeZone();
				var myScheduleAtPersonToLocalPeriod = new DateTimePeriod( TimeZoneHelper.ConvertFromUtc(myScheduleStart, personToTimezone), 
																		TimeZoneHelper.ConvertFromUtc(myScheduleEnd, personToTimezone));
				var personToOpenPeriod = new DateTimePeriod(
					new DateTime(date.Year, date.Month, date.Day, personToOpenStart.Hours, personToOpenStart.Minutes, personToOpenStart.Seconds, DateTimeKind.Utc), 
					new DateTime(date.Year, date.Month, date.Day, personToOpenEnd.Hours, personToOpenEnd.Minutes, personToOpenEnd.Seconds, DateTimeKind.Utc));
				if (!personToOpenPeriod.Contains(myScheduleAtPersonToLocalPeriod) || personToSiteOpenHour.IsClosed) return true;
			}

			var mySiteOpenHours = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			var myOpenStart = mySiteOpenHours.TimePeriod.StartTime;
			var myOpenEnd = mySiteOpenHours.TimePeriod.EndTime;
			var personToShiftLayers = personToScheduleDay?.PersonAssignment().ShiftLayers;
			if (personToShiftLayers != null || personToShiftLayers.Any())
			{
				var personToScheduleStart = personToShiftLayers.First().Period.StartDateTime;
				var personToScheduleEnd = personToShiftLayers.Last().Period.EndDateTime;
				var myTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				var personToScheduleAtMyLocalPeriod = new DateTimePeriod(TimeZoneHelper.ConvertFromUtc(personToScheduleStart, myTimezone),
																		TimeZoneHelper.ConvertFromUtc(personToScheduleEnd, myTimezone));
				var myOpenPeriod = new DateTimePeriod(
					new DateTime(date.Year, date.Month, date.Day, myOpenStart.Hours, myOpenStart.Minutes, myOpenStart.Seconds, DateTimeKind.Utc), 
					new DateTime(date.Year, date.Month, date.Day, myOpenEnd.Hours, myOpenEnd.Minutes, myOpenEnd.Seconds, DateTimeKind.Utc));
				if (!myOpenPeriod.Contains(personToScheduleAtMyLocalPeriod) || mySiteOpenHours.IsClosed) return true;
			}

			return false;
		}

		private bool isSkillSatisfied(IPerson personTo, DateOnly date)
		{
			var checkItem = new ShiftTradeAvailableCheckItem(date, _loggedOnUser.CurrentUser(), personTo);
			var skillSpecification = new ShiftTradeSkillSpecification();
			return skillSpecification.IsSatisfiedBy(checkItem);
		}

		private bool isDateSatisfied(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, DateOnly date, IPerson personTo)
		{
			var personToFirstLayer = personToScheduleDay?.PersonAssignment()?.ShiftLayers.FirstOrDefault();
			var myTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var personTostartTime = TimeZoneHelper.ConvertToUtc(date.Date, myTimezone);
			if (personToFirstLayer != null) personTostartTime = personToFirstLayer.Period.StartDateTime;

			var personToTimeZone = personTo.PermissionInformation.DefaultTimeZone();
			var myfirstLayer = myScheduleDay?.PersonAssignment()?.ShiftLayers.FirstOrDefault();
			var myStartTime = TimeZoneHelper.ConvertToUtc(date.Date, personToTimeZone);
			if (myfirstLayer != null) myStartTime = myfirstLayer.Period.StartDateTime;

			var startInMyTimezone = TimeZoneHelper.ConvertFromUtc(personTostartTime, myTimezone);
			var startInPersonToTimezone = TimeZoneHelper.ConvertFromUtc(myStartTime, personToTimeZone);
			if (startInMyTimezone < date.Date || startInMyTimezone >= date.Date.AddHours(24)
											  || startInPersonToTimezone < date.Date || startInPersonToTimezone >= date.Date.AddDays(24))
			{
				return false;
			}

			return true;
		}
	}
}