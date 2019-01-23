using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions
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
		private readonly IPersonalShiftRestrictionCombiner _personalShiftRestrictionCombiner;
		private readonly IMeetingRestrictionCombiner _meetingRestrictionCombiner;

		public AgentRestrictionsDetailEffectiveRestrictionExtractor(IWorkShiftWorkTime workShiftWorkTime, IRestrictionExtractor restrictionExtractor, RestrictionSchedulingOptions schedulingOptions, IPersonalShiftRestrictionCombiner personalShiftRestrictionCombiner, IMeetingRestrictionCombiner meetingRestrictionCombiner)
		{
			_workShiftWorkTime = workShiftWorkTime;
			_restrictionExtractor = restrictionExtractor;
			_schedulingOptions = schedulingOptions;
			_meetingRestrictionCombiner = meetingRestrictionCombiner;
			_personalShiftRestrictionCombiner = personalShiftRestrictionCombiner;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Extract(IScheduleMatrixPro scheduleMatrixPro, PreferenceCellData preferenceCellData, DateOnly dateOnly, DateTimePeriod loadedPeriod, TimeSpan periodTarget)
		{
			if(scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");
			if(preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			var scheduleDay = scheduleMatrixPro.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			
			preferenceCellData.TheDate = dateOnly;
			preferenceCellData.SchedulePart = scheduleDay;
			preferenceCellData.Enabled = loadedPeriod.Contains(dateOnly.Date) && scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly);
			preferenceCellData.PeriodTarget = periodTarget;
			preferenceCellData.SchedulingOption = _schedulingOptions;

			var result = _restrictionExtractor.Extract(scheduleDay);
			var totalRestriction = result.CombinedRestriction(_schedulingOptions);

			totalRestriction = _personalShiftRestrictionCombiner.Combine(scheduleDay, totalRestriction);
			totalRestriction = _meetingRestrictionCombiner.Combine(scheduleDay, totalRestriction);

			if (_schedulingOptions.UseScheduling)
			{
				if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.MainShift)) totalRestriction = ExtractOnMainShift(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.DayOff)) totalRestriction = ExtractOnDayOff(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.FullDayAbsence)) totalRestriction = ExtractFullDayAbsence(scheduleDay, preferenceCellData);
				if (scheduleDay.SignificantPartForDisplay().Equals(SchedulePartView.ContractDayOff)) totalRestriction = ExtractFullDayAbsence(scheduleDay, preferenceCellData);
			}

			SetTotalRestriction(scheduleDay, totalRestriction, preferenceCellData);

			preferenceCellData.MustHavePreference = result.PreferenceList.Any(restriction => restriction.MustHave);

			foreach (var restriction in result.PreferenceList)
			{
				if (restriction.WorkTimeLimitation.HasValue() || restriction.StartTimeLimitation.HasValue() || restriction.EndTimeLimitation.HasValue() || restriction.ActivityRestrictionCollection.Count > 0)
				{
					preferenceCellData.HasExtendedPreference = true;
					break;
				}
			}


			var personPeriod = scheduleMatrixPro.Person.Period(dateOnly);
			if (personPeriod == null)
			{
				preferenceCellData.WeeklyMax = new TimeSpan(0);
                preferenceCellData.WeeklyMin = new TimeSpan(0);
				preferenceCellData.NightlyRest = new TimeSpan(0);
			}
			else
			{
				var worktimeDirective = personPeriod.PersonContract.Contract.WorkTimeDirective;
				preferenceCellData.WeeklyMax = worktimeDirective.MaxTimePerWeek;
			    preferenceCellData.WeeklyMin = worktimeDirective.MinTimePerWeek;
				preferenceCellData.NightlyRest = worktimeDirective.NightlyRest;
			    preferenceCellData.EmploymentType = personPeriod.PersonContract.Contract.EmploymentType;
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
			
			if (scheduleDay.SignificantPartForDisplay() == SchedulePartView.ContractDayOff) 
				preferenceCellData.HasAbsenceOnContractDayOff = true;

            WorkTimeLimitation workTimeLimitation;
			if (preferenceCellData.HasAbsenceOnContractDayOff)
				workTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero);
            else workTimeLimitation = new WorkTimeLimitation(timeSpan, timeSpan);

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),new WorkTimeLimitation(), null, null, null,new List<IActivityRestriction>());
            totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null,null, null, new List<IActivityRestriction>()));

			var description = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDescription(scheduleDay.Person);
			preferenceCellData.DisplayName = description.Name;
			preferenceCellData.DisplayShortName = description.ShortName;
			preferenceCellData.DisplayColor = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDisplayColor(scheduleDay.Person);
			preferenceCellData.HasFullDayAbsence = true;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(),TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			preferenceCellData.StartEndScheduledShift = period.Value.TimePeriod(TimeZoneGuard.Instance.TimeZone).ToShortTimeString(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);

            return totalRestriction;
		}

		private IEffectiveRestriction ExtractOnDayOff(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var endTimeLimitation = new EndTimeLimitation(null, null);
			var workTimeLimitation = new WorkTimeLimitation(null, null);
			var dayOff = scheduleDay.PersonAssignment().DayOff();
			var dayOffTemplate = new DayOffTemplate(dayOff.Description);
			dayOffTemplate.SetTargetAndFlexibility(dayOff.TargetLength, dayOff.Flexibility);

			IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),new WorkTimeLimitation(), null, null, null,new List<IActivityRestriction>());

			totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null, dayOffTemplate, null, new List<IActivityRestriction>()));
			preferenceCellData.DisplayName = dayOff.Description.Name;
			preferenceCellData.DisplayShortName = dayOff.Description.ShortName;
			preferenceCellData.HasDayOff = true;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(dayOffTemplate.TargetLength, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			
			return totalRestriction;
		}

		private IEffectiveRestriction ExtractOnMainShift(IScheduleDay scheduleDay, IPreferenceCellData preferenceCellData)
		{
			IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var projection = scheduleDay.ProjectionService().CreateProjection();
			var timePeriod = projection.Period();
			if (timePeriod == null) return null;
			var dateTimePeriod = timePeriod.Value;
			var zone = TimeZoneGuard.Instance.TimeZone;
			var viewLocalTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.StartDateTime, zone);
			var viewLocalEndTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.EndDateTime, zone);
			var startTimeLimitation = new StartTimeLimitation(viewLocalTime.TimeOfDay, viewLocalTime.TimeOfDay);
			var endTimeStart = viewLocalEndTime.Subtract(viewLocalTime.Date);
			var endTimeLimitation = new EndTimeLimitation(endTimeStart, endTimeStart);
			//bug 25880 crashed if the shift was 24h or longer
			var workTimeLimitation = new WorkTimeLimitation(null, null);
			var contractTime = projection.ContractTime();

			totalRestriction = totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null, null, null, new List<IActivityRestriction>()));

			var assignment = scheduleDay.PersonAssignment();
			preferenceCellData.HasShift = true;
			preferenceCellData.DisplayName = assignment.ShiftCategory.Description.Name;
			preferenceCellData.DisplayShortName = assignment.ShiftCategory.Description.ShortName;
			preferenceCellData.DisplayColor = assignment.ShiftCategory.DisplayColor;
			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(contractTime, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
			var period = projection.Period();
			if (period != null) preferenceCellData.StartEndScheduledShift = period.Value.TimePeriod(TimeZoneGuard.Instance.TimeZone).ToShortTimeString(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);

			return totalRestriction;
		}

		private void SetTotalRestriction(IScheduleDay scheduleDay, IEffectiveRestriction totalRestriction, IPreferenceCellData preferenceCellData)
		{
			var minMaxLength = GetMinMaxLength(scheduleDay, totalRestriction);
			if (minMaxLength != null)
				totalRestriction = new EffectiveRestriction(minMaxLength.StartTimeLimitation, minMaxLength.EndTimeLimitation,
				                                            minMaxLength.WorkTimeLimitation, totalRestriction.ShiftCategory,
				                                            totalRestriction.DayOffTemplate, totalRestriction.Absence,
				                                            new List<IActivityRestriction>());
			else
			{
				if (totalRestriction != null && totalRestriction.IsRestriction && totalRestriction.DayOffTemplate == null &&
				    totalRestriction.Absence == null)
				{
					preferenceCellData.NoShiftsCanBeFound = true;
					totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
					                                            new WorkTimeLimitation(), totalRestriction.ShiftCategory,
					                                            totalRestriction.DayOffTemplate, totalRestriction.Absence,
					                                            new List<IActivityRestriction>());
				}
			}
			preferenceCellData.EffectiveRestriction = totalRestriction;
			if (totalRestriction != null && totalRestriction.Absence != null)
				SetTotalRestrictionForPreferedAbsence(scheduleDay, preferenceCellData);	
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

			preferenceCellData.HasAbsenceOnContractDayOff = 
				!virtualSchedulePeriod.ContractSchedule.IsWorkday(schedulePeriodStartDate.Value, preferenceCellData.TheDate, scheduleDay.Person.FirstDayOfWeek);

			var time = TimeSpan.Zero;
			if (!preferenceCellData.HasAbsenceOnContractDayOff && preferenceCellData.EffectiveRestriction.Absence.InContractTime) 
				time = virtualSchedulePeriod.AverageWorkTimePerDay;

			preferenceCellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(time, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
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
