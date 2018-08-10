using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public ShiftTradeMultiSchedulesSelectableChecker(ILoggedOnUser loggedOnUser, IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_loggedOnUser = loggedOnUser;
			_globalSettingDataRepository = globalSettingDataRepository;
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

			if (hasNonMainShiftActivities(myScheduleDay, personToScheduleDay))
			{
				unSelectableReason = Resources.NotAllowedWhenHasNonMainShiftAcitivities;
				return false;
			}

			return true;
		}

		private bool isRuleNeedToCheck(string ruleType)
		{
			var allRlues = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings()).BusinessRuleConfigs;

			var currentRule =  allRlues.First(x => x.BusinessRuleType == ruleType);
			if (currentRule == null || !currentRule.Enabled || currentRule.HandleOptionOnFailed != RequestHandleOption.AutoDeny) return false;

			return true;
		}

		private bool hasNonMainShiftActivities(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay)
		{
			if(!isRuleNeedToCheck(typeof(NonMainShiftActivityRule).FullName)) return false;

			var myPersonAssignment = myScheduleDay?.PersonAssignment();
			var meeting = myScheduleDay?.PersonMeetingCollection();
			var overTime = myPersonAssignment?.OvertimeActivities();
			var personalActivity = myPersonAssignment?.PersonalActivities();
			if (hasMeeting(meeting) || hasNonMainShiftLayer(overTime) || hasNonMainShiftLayer(personalActivity)) return true;

			var personToAssignment = personToScheduleDay?.PersonAssignment();
			var personToMeeting = personToScheduleDay?.PersonMeetingCollection();
			var personToOverTime = personToAssignment.OvertimeActivities();
			var personToAcitiviy = personToAssignment.PersonalActivities();
			if (hasMeeting(personToMeeting) || hasNonMainShiftLayer(personToOverTime) || hasNonMainShiftLayer(personToAcitiviy)) return true;

			return false;
		}

		private bool hasMeeting(IPersonMeeting[] meeting)
		{
			if (meeting != null && meeting.Any()) return true;

			return false;
		}

		private bool hasNonMainShiftLayer(IEnumerable<ShiftLayer> shiftLayers)
		{
			if (shiftLayers != null && shiftLayers.Any()) return true;

			return false;
		}

		private bool isOutsideOpenHours(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, IPerson personTo, DateOnly date)
		{
			var personToSiteOpenHour = personTo.SiteOpenHour(date);
			if (personToSiteOpenHour != null)
			{
				var personToOpenStart = personToSiteOpenHour.TimePeriod.StartTime;
				var personToOpenEnd = personToSiteOpenHour.TimePeriod.EndTime;
				var myShiftLayers = myScheduleDay?.PersonAssignment()?.ShiftLayers;
				if (myShiftLayers != null || myShiftLayers.Any())
				{
					var myScheduleStart = myShiftLayers.First().Period.StartDateTime;
					var myScheduleEnd = myShiftLayers.Last().Period.EndDateTime;
					var personToTimezone = personTo.PermissionInformation.DefaultTimeZone();
					var myScheduleAtPersonToLocalPeriod = new DateTimePeriod(TimeZoneHelper.ConvertFromUtc(myScheduleStart, personToTimezone),
																			TimeZoneHelper.ConvertFromUtc(myScheduleEnd, personToTimezone));
					var personToOpenPeriod = new DateTimePeriod(
						new DateTime(date.Year, date.Month, date.Day, personToOpenStart.Hours, personToOpenStart.Minutes, personToOpenStart.Seconds, DateTimeKind.Utc),
						new DateTime(date.Year, date.Month, date.Day, personToOpenEnd.Hours, personToOpenEnd.Minutes, personToOpenEnd.Seconds, DateTimeKind.Utc));
					if (myScheduleAtPersonToLocalPeriod.StartDateTime < personToOpenPeriod.StartDateTime
						|| myScheduleAtPersonToLocalPeriod.EndDateTime > personToOpenPeriod.EndDateTime 
						|| personToSiteOpenHour.IsClosed) return true;
				}

			}

			var mySiteOpenHours = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			if (mySiteOpenHours !=  null)
			{
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
					if (personToScheduleAtMyLocalPeriod.StartDateTime < myOpenPeriod.StartDateTime
						|| personToScheduleAtMyLocalPeriod.EndDateTime > myOpenPeriod.EndDateTime
						|| mySiteOpenHours.IsClosed) return true;
				}
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