﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDetailEffectiveRestrictionExtractor
	{
		void Extract(IScheduleMatrixPro scheduleMatrixPro, PreferenceCellData preferenceCellData,DateOnly dateOnly, DateTimePeriod loadedPeriod, TimeSpan periodTarget);
	}

	public class AgentRestrictionsDetailEffectiveRestrictionExtractor : IAgentRestrictionsDetailEffectiveRestrictionExtractor
	{
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly RestrictionSchedulingOptions _schedulingOptions;

		public AgentRestrictionsDetailEffectiveRestrictionExtractor(IWorkShiftWorkTime workShiftWorkTime, IRestrictionExtractor restrictionExtractor, RestrictionSchedulingOptions schedulingOptions)
		{
			_workShiftWorkTime = workShiftWorkTime;
			_restrictionExtractor = restrictionExtractor;
			_schedulingOptions = schedulingOptions;
		}

		public void Extract(IScheduleMatrixPro scheduleMatrixPro, PreferenceCellData preferenceCellData, DateOnly dateOnly, DateTimePeriod loadedPeriod, TimeSpan periodTarget)
		{
			if(scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");
			if(preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			var scheduleDay = scheduleMatrixPro.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			
			preferenceCellData.TheDate = dateOnly;
			preferenceCellData.SchedulePart = scheduleDay;
			preferenceCellData.Enabled = loadedPeriod.Contains(dateOnly) && scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly);
			preferenceCellData.PeriodTarget = periodTarget;
			preferenceCellData.SchedulingOption = _schedulingOptions;

			_restrictionExtractor.Extract(scheduleMatrixPro.Person, dateOnly);
			var totalRestriction = _restrictionExtractor.CombinedRestriction(_schedulingOptions);

			if (_schedulingOptions.UseScheduling)
			{
				if (scheduleDay.SignificantPart().Equals(SchedulePartView.MainShift)) totalRestriction = ExtractOnMainShift(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPart().Equals(SchedulePartView.DayOff)) totalRestriction = ExtractOnDayOff(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPart().Equals(SchedulePartView.FullDayAbsence)) totalRestriction = ExtractFullDayAbsence(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPart().Equals(SchedulePartView.ContractDayOff)) totalRestriction = ExtractFullDayAbsence(scheduleDay, preferenceCellData);
			}

			SetTotalRestriction(scheduleDay, totalRestriction, preferenceCellData);

			preferenceCellData.MustHavePreference = _restrictionExtractor.PreferenceList.Any(restriction => restriction.MustHave);

			if (scheduleMatrixPro.Person.Period(dateOnly) == null)
			{
				preferenceCellData.WeeklyMax = new TimeSpan(0);
				preferenceCellData.NightlyRest = new TimeSpan(0);
			}
			else
			{
				var worktimeDirective = scheduleMatrixPro.Person.Period(dateOnly).PersonContract.Contract.WorkTimeDirective;
				preferenceCellData.WeeklyMax = worktimeDirective.MaxTimePerWeek;
				preferenceCellData.NightlyRest = worktimeDirective.NightlyRest;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private IEffectiveRestriction ExtractFullDayAbsence(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();
			var period = projection.Period();
			if (period == null) return null;
            var timeSpan = period.Value.ElapsedTime();
            var startTimeLimitation = new StartTimeLimitation(null, null);
            var endTimeLimitation = new EndTimeLimitation(null, null);
			var virtualSchedulePeriod = scheduleDay.Person.VirtualSchedulePeriod(scheduleDay.DateOnlyAsPeriod.DateOnly);
			var schedulePeriodStart = scheduleDay.Person.SchedulePeriodStartDate(scheduleDay.DateOnlyAsPeriod.DateOnly);

			if (virtualSchedulePeriod.IsValid && schedulePeriodStart.HasValue) preferenceCellData.HasAbsenceOnContractDayOff = !virtualSchedulePeriod.ContractSchedule.IsWorkday(schedulePeriodStart.Value, scheduleDay.DateOnlyAsPeriod.DateOnly);
            
            WorkTimeLimitation workTimeLimitation;
			if (preferenceCellData.HasAbsenceOnContractDayOff)workTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero);
            else workTimeLimitation = new WorkTimeLimitation(timeSpan, timeSpan);

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),new WorkTimeLimitation(), null, null, null,new List<IActivityRestriction>());
            totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null,null, null, new List<IActivityRestriction>()));

			preferenceCellData.DisplayName = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDescription(scheduleDay.Person).Name;
			preferenceCellData.DisplayShortName = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDescription(scheduleDay.Person).ShortName;
			preferenceCellData.DisplayColor = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDisplayColor(scheduleDay.Person);
			preferenceCellData.HasFullDayAbsence = true;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(),TeleoptiPrincipal.Current.Regional.Culture);
			preferenceCellData.StartEndScheduledShift = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).ToShortTimeString(TeleoptiPrincipal.Current.Regional.Culture);
            
            return totalRestriction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private IEffectiveRestriction ExtractOnDayOff(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(null, null);
			var dayOffTemplate = new DayOffTemplate(scheduleDay.PersonDayOffCollection()[0].DayOff.Description);
			dayOffTemplate.SetTargetAndFlexibility(scheduleDay.PersonDayOffCollection()[0].DayOff.TargetLength, scheduleDay.PersonDayOffCollection()[0].DayOff.Flexibility);

			IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),new WorkTimeLimitation(), null, null, null,new List<IActivityRestriction>());

			totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null, dayOffTemplate, null, new List<IActivityRestriction>()));
			preferenceCellData.DisplayName = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.Name;
			preferenceCellData.DisplayShortName = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.ShortName;
			preferenceCellData.HasDayOff = true;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(dayOffTemplate.TargetLength, TeleoptiPrincipal.Current.Regional.Culture);
			
			return totalRestriction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private IEffectiveRestriction ExtractOnMainShift(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var projection = scheduleDay.ProjectionService().CreateProjection();
			var timePeriod = projection.Period();
			if (timePeriod == null) return null;
			var dateTimePeriod = timePeriod.Value;
			var zone = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
			var viewLocalTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.StartDateTime, zone);
			var viewLocalEndTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.EndDateTime, zone);
			var startTimeLimitation = new StartTimeLimitation(viewLocalTime.TimeOfDay, viewLocalTime.TimeOfDay);
			var daysToAdd = 0;
			if (viewLocalEndTime.Date > viewLocalTime.Date) daysToAdd = 1;
			var endTimeStart = new TimeSpan(daysToAdd, viewLocalEndTime.TimeOfDay.Hours, viewLocalEndTime.TimeOfDay.Minutes, 0);
			var endTimeLimitation = new EndTimeLimitation(endTimeStart, endTimeStart);
			var workTimeLimitation = new WorkTimeLimitation(projection.ContractTime(), projection.ContractTime());

			totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null, null, null, new List<IActivityRestriction>()));

			preferenceCellData.HasShift = true;
			preferenceCellData.DisplayName = scheduleDay.PersonAssignmentCollection()[0].MainShift.ShiftCategory.Description.Name;
			preferenceCellData.DisplayShortName = scheduleDay.PersonAssignmentCollection()[0].MainShift.ShiftCategory.Description.ShortName;
			preferenceCellData.DisplayColor = scheduleDay.PersonAssignmentCollection()[0].MainShift.ShiftCategory.DisplayColor;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), TeleoptiPrincipal.Current.Regional.Culture);
			var period = projection.Period();
			if (period != null) preferenceCellData.StartEndScheduledShift = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).ToShortTimeString(TeleoptiPrincipal.Current.Regional.Culture);

			return totalRestriction;
		}

		private void SetTotalRestriction(IScheduleDay scheduleDay, IEffectiveRestriction totalRestriction, IPreferenceCellData preferenceCellData)
		{
			var minMaxLength = GetMinMaxLength(scheduleDay, totalRestriction);
			if (minMaxLength != null)totalRestriction = new EffectiveRestriction(minMaxLength.StartTimeLimitation,minMaxLength.EndTimeLimitation,minMaxLength.WorkTimeLimitation,totalRestriction.ShiftCategory,totalRestriction.DayOffTemplate,totalRestriction.Absence, new List<IActivityRestriction>());
			else
			{
				if (totalRestriction.IsRestriction && totalRestriction.DayOffTemplate == null && totalRestriction.Absence == null)
				{
					preferenceCellData.NoShiftsCanBeFound = true;
					totalRestriction = new EffectiveRestriction(new StartTimeLimitation(),new EndTimeLimitation(),new WorkTimeLimitation(),totalRestriction.ShiftCategory,totalRestriction.DayOffTemplate,totalRestriction.Absence, new List<IActivityRestriction>());
				}
			}
			preferenceCellData.EffectiveRestriction = totalRestriction;
			if (totalRestriction.Absence != null) SetTotalRestrictionForPreferedAbsence(scheduleDay, preferenceCellData);	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private void SetTotalRestrictionForPreferedAbsence(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			var virtualSchedulePeriod = scheduleDay.Person.VirtualSchedulePeriod(preferenceCellData.TheDate);
			if (!virtualSchedulePeriod.IsValid) return;

			var personPeriod = scheduleDay.Person.Period(preferenceCellData.TheDate);
			if (personPeriod == null) return;

			var schedulePeriodStartDate = scheduleDay.Person.SchedulePeriodStartDate(preferenceCellData.TheDate);
			if (!schedulePeriodStartDate.HasValue) return;

			preferenceCellData.HasAbsenceOnContractDayOff = !virtualSchedulePeriod.ContractSchedule.IsWorkday(schedulePeriodStartDate.Value, preferenceCellData.TheDate);

			var time = TimeSpan.Zero;
			if (!preferenceCellData.HasAbsenceOnContractDayOff && preferenceCellData.EffectiveRestriction.Absence.InContractTime) time = virtualSchedulePeriod.AverageWorkTimePerDay;

			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(time, TeleoptiPrincipal.Current.Regional.Culture);
			var totalRestriction = new EffectiveRestriction(preferenceCellData.EffectiveRestriction.StartTimeLimitation, preferenceCellData.EffectiveRestriction.EndTimeLimitation, new WorkTimeLimitation(time, time), preferenceCellData.EffectiveRestriction.ShiftCategory, preferenceCellData.EffectiveRestriction.DayOffTemplate, preferenceCellData.EffectiveRestriction.Absence, new List<IActivityRestriction>());
			preferenceCellData.EffectiveRestriction = totalRestriction;
		}

		private IWorkTimeMinMax GetMinMaxLength(IScheduleDay scheduleDay, IEffectiveRestriction totalRestriction)
		{
			var personPeriod = scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly);
			if (personPeriod == null) return null;
			var ruleSetBag = personPeriod.RuleSetBag;
			if (ruleSetBag == null) return null;

			return ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, scheduleDay.DateOnlyAsPeriod.DateOnly, totalRestriction);
		}
	}
}
