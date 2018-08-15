﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
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
		private readonly INow _now;
		private readonly ISiteOpenHoursSpecification _siteOpenHoursSpecification;

		public ShiftTradeMultiSchedulesSelectableChecker(ILoggedOnUser loggedOnUser, IGlobalSettingDataRepository globalSettingDataRepository, INow now, ISiteOpenHoursSpecification siteOpenHoursSpecification)
		{
			_loggedOnUser = loggedOnUser;
			_globalSettingDataRepository = globalSettingDataRepository;
			_now = now;
			_siteOpenHoursSpecification = siteOpenHoursSpecification;
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

			if (isOutOpenPeriod(personTo, date, out var agentName))
			{
				unSelectableReason = String.Format(Resources.DateOutOfShiftTradeOpenPeriod, agentName);
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

			if (isOutsideOpenHours(myScheduleDay, personToScheduleDay, personTo))
			{
				unSelectableReason = Resources.NotAllowedWhenShiftOutsideOpenHours;
				return false;
			}

			if (hasNonMainShiftActivities(myScheduleDay, personToScheduleDay))
			{
				unSelectableReason = Resources.NotAllowedWhenHasNonMainShiftAcitivities;
				return false;
			}

			if (hasNonOverwriteActivities(myScheduleDay, personToScheduleDay))
			{
				unSelectableReason = Resources.NotAllowedWhenHasNonOverwriteActivity;
				return false;
			}

			return true;
		}

		private bool isOutOpenPeriod(IPerson personTo, DateOnly date, out Name agentName)
		{
			agentName = new Name();
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var myToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));
			var myOpenPeriodStart = myToday.AddDays(_loggedOnUser.CurrentUser().WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum);
			var myOpenPeriodEnd = myToday.AddDays(_loggedOnUser.CurrentUser().WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum);
			if (date < myOpenPeriodStart || date > myOpenPeriodEnd)
			{
				agentName = _loggedOnUser.CurrentUser().Name;
				return true;
			}

			var personToTimeZone = personTo.PermissionInformation.DefaultTimeZone();
			var personToToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), personToTimeZone));
			var personToStart = personToToday.AddDays(personTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum);
			var personToEnd = personToToday.AddDays(personTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum);
			if (date < personToStart || date > personToEnd)
			{
				agentName = personTo.Name;
				return true;
			}

			return false;
		}

		private bool hasNonOverwriteActivities(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay)
		{
			if (!isRuleNeedToCheck(typeof(NotOverwriteLayerRule).FullName)) return false;

			if (hasMeetingOrPersonalActivity(myScheduleDay)) return true;
			if (hasMeetingOrPersonalActivity(personToScheduleDay)) return true;

			return false;
		}

		private bool hasMeetingOrPersonalActivity(IScheduleDay scheduleDay)
		{
			if (scheduleDay == null) return false;

			var rule = new NotOverwriteLayerRule();
			var overLapingLayers = rule.GetOverlappingLayerses(scheduleDay);

			return overLapingLayers.Any();
		}

		private bool isRuleNeedToCheck(string ruleType)
		{
			var allRlues = _globalSettingDataRepository.FindValueByKey(ShiftTradeSettings.SettingsKey,
				new ShiftTradeSettings()).BusinessRuleConfigs;

			var currentRule =  allRlues?.FirstOrDefault(x => x.BusinessRuleType == ruleType);
			if (currentRule == null || !currentRule.Enabled || currentRule.HandleOptionOnFailed != RequestHandleOption.AutoDeny) return false;

			return true;
		}

		private bool hasNonMainShiftActivities(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay)
		{
			if(!isRuleNeedToCheck(typeof(NonMainShiftActivityRule).FullName)) return false;

			var overTime = myScheduleDay?.PersonAssignment()?.OvertimeActivities();
			if (hasOvertime(overTime)) return true;

			var personToOverTime = personToScheduleDay?.PersonAssignment().OvertimeActivities();
			if (hasOvertime(personToOverTime)) return true;

			if (hasMeetingOrPersonalActivity(myScheduleDay)) return true;
			if (hasMeetingOrPersonalActivity(personToScheduleDay)) return true;

			return false;
		}

		private bool hasOvertime(IEnumerable<OvertimeShiftLayer> overtime)
		{
			if (overtime != null && overtime.Any()) return true;

			return false;
		}

		private bool isOutsideOpenHours(IScheduleDay myScheduleDay, IScheduleDay personToScheduleDay, IPerson personTo)
		{
			var layerCollection = myScheduleDay?.ProjectionService().CreateProjection();
			if (layerCollection != null && layerCollection.HasLayers)
			{
				var period = layerCollection.Period().Value;
				var checkItem = new SiteOpenHoursCheckItem { Period = period, Person = personTo };
				if (!_siteOpenHoursSpecification.IsSatisfiedBy(checkItem)) return true;
			}

			var personToLayers = personToScheduleDay?.ProjectionService().CreateProjection();
			if (personToLayers != null && personToLayers.HasLayers)
			{
				var period = personToLayers.Period().Value;
				var checkItem = new SiteOpenHoursCheckItem { Period = period, Person = _loggedOnUser.CurrentUser() };
				if (!_siteOpenHoursSpecification.IsSatisfiedBy(checkItem)) return true;
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