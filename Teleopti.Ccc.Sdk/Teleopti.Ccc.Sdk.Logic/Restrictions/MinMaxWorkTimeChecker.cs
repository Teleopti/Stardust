using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public interface IMinMaxWorkTimeChecker
    {
        IWorkTimeMinMax MinMaxWorkTime(IScheduleDay scheduleDay, IRuleSetBag ruleSetBag, IEffectiveRestriction effectiveRestriction, bool useContractTimeOnMainShift);
    }

    public class MinMaxWorkTimeChecker : IMinMaxWorkTimeChecker
    {
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;

    	public MinMaxWorkTimeChecker(IWorkShiftWorkTime workShiftWorkTime)
        {
        	if (workShiftWorkTime == null)
				  throw new ArgumentNullException(nameof(workShiftWorkTime));
			_workShiftWorkTime = workShiftWorkTime;
        }

		public IWorkTimeMinMax MinMaxWorkTime(IScheduleDay scheduleDay, IRuleSetBag ruleSetBag, IEffectiveRestriction effectiveRestriction, bool useContractTimeOnMainShift)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException(nameof(scheduleDay));

            if (ruleSetBag == null)
                throw new ArgumentNullException(nameof(ruleSetBag));

            var significant = scheduleDay.SignificantPart();

            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff || (effectiveRestriction!=null && effectiveRestriction.NotAvailable))
                return new WorkTimeMinMax();

            if (significant == SchedulePartView.MainShift)
            {
				return useContractTimeOnMainShift ? GetContractTime(scheduleDay) : GetWorkTime(scheduleDay);
            }

    		if (significant == SchedulePartView.FullDayAbsence)
			{
				return GetContractTime(scheduleDay);
			}

            if (effectiveRestriction?.Absence != null)
            {
                return GetWorkTimeAbsencePreference(scheduleDay, effectiveRestriction);
            }

			effectiveRestriction = new PersonalShiftRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, effectiveRestriction);
			effectiveRestriction = new MeetingRestrictionCombiner(new RestrictionCombiner()).Combine(scheduleDay, effectiveRestriction);

            var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
				return ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, dateOnly, effectiveRestriction);
           
        }

        public static IWorkTimeMinMax GetWorkTimeAbsencePreference(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException(nameof(scheduleDay));

            if(effectiveRestriction == null)
                throw new ArgumentNullException(nameof(effectiveRestriction));

            var person = scheduleDay.Person;
            var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
	        var averageWorkTimeOfDay = person.AverageWorkTimeOfDay(scheduleDate);
	        var avgWorkTime = new TimeSpan((long)(averageWorkTimeOfDay.AverageWorkTime.Value.Ticks  * averageWorkTimeOfDay.PartTimePercentage.Value));

			if (!averageWorkTimeOfDay.IsWorkDay)
                return new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero)};

            if(!effectiveRestriction.Absence.InContractTime)
                return new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero) };

            IWorkTimeMinMax minMaxLength = new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime) };

            return minMaxLength;
        }

        public static IWorkTimeMinMax GetWorkTime(IScheduleDay scheduleDay)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException(nameof(scheduleDay));

            TimeZoneInfo timeZoneInfo = scheduleDay.TimeZone;
            IWorkTimeMinMax minMaxLength = new WorkTimeMinMax();
            IProjectionService projSvc = scheduleDay.ProjectionService();
            var proj = projSvc.CreateProjection();
            TimeSpan workTime = proj.WorkTime();
            minMaxLength.WorkTimeLimitation = new WorkTimeLimitation(workTime, workTime);
            
			var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
	        var shiftStartTime = workTimeStartEndExtractor.WorkTimeStart(proj);
	        var shiftEndTime = workTimeStartEndExtractor.WorkTimeEnd(proj);

			if (shiftStartTime.HasValue && shiftEndTime.HasValue)
			{
	            var localStartDateTime = TimeZoneHelper.ConvertFromUtc(shiftStartTime.Value,timeZoneInfo);
				var endTime = TimeZoneHelper.ConvertFromUtc(shiftEndTime.Value, timeZoneInfo).Subtract(localStartDateTime.Date);
                minMaxLength.StartTimeLimitation = new StartTimeLimitation(localStartDateTime.TimeOfDay, localStartDateTime.TimeOfDay);
                minMaxLength.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);
            }

            return minMaxLength;
        }
		
		public static IWorkTimeMinMax GetContractTime(IScheduleDay scheduleDay)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException(nameof(scheduleDay));

            TimeZoneInfo timeZoneInfo = scheduleDay.TimeZone;
            IWorkTimeMinMax minMaxLength = new WorkTimeMinMax();
            IProjectionService projSvc = scheduleDay.ProjectionService();
            var proj = projSvc.CreateProjection();
            TimeSpan workTime = proj.ContractTime();
            minMaxLength.WorkTimeLimitation = new WorkTimeLimitation(workTime, workTime);
            
			var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
	        var shiftStartTime = workTimeStartEndExtractor.WorkTimeStart(proj);
	        var shiftEndTime = workTimeStartEndExtractor.WorkTimeEnd(proj);

			if (shiftStartTime.HasValue && shiftEndTime.HasValue)
			{
	            var localStartDateTime = TimeZoneHelper.ConvertFromUtc(shiftStartTime.Value,timeZoneInfo);
				var endTime = TimeZoneHelper.ConvertFromUtc(shiftEndTime.Value, timeZoneInfo).Subtract(localStartDateTime.Date);
                minMaxLength.StartTimeLimitation = new StartTimeLimitation(localStartDateTime.TimeOfDay, localStartDateTime.TimeOfDay);
                minMaxLength.EndTimeLimitation = new EndTimeLimitation(endTime, endTime);
            }

            return minMaxLength;
        }
    }
}