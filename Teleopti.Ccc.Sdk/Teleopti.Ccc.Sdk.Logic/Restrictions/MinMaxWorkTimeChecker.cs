using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public interface IMinMaxWorkTimeChecker
    {
        IWorkTimeMinMax MinMaxWorkTime(IScheduleDay scheduleDay, IRuleSetBag ruleSetBag, IEffectiveRestriction effectiveRestriction);
    }

    public class MinMaxWorkTimeChecker : IMinMaxWorkTimeChecker
    {
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;

    	public MinMaxWorkTimeChecker(IWorkShiftWorkTime workShiftWorkTime)
        {
        	if (workShiftWorkTime == null)
				  throw new ArgumentNullException("workShiftWorkTime");
			_workShiftWorkTime = workShiftWorkTime;
        }

    	public IWorkTimeMinMax MinMaxWorkTime(IScheduleDay scheduleDay, IRuleSetBag ruleSetBag, IEffectiveRestriction effectiveRestriction)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException("scheduleDay");

            if (ruleSetBag == null)
                throw new ArgumentNullException("ruleSetBag");

            var significant = scheduleDay.SignificantPart();

            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff || (effectiveRestriction!=null && effectiveRestriction.NotAvailable))
                return new WorkTimeMinMax();

            if (significant == SchedulePartView.MainShift || significant == SchedulePartView.FullDayAbsence)
            {
                return GetWorkTime(scheduleDay);
            }

            if (effectiveRestriction != null && effectiveRestriction.Absence != null)
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
                throw new ArgumentNullException("scheduleDay");

            if(effectiveRestriction == null)
                throw new ArgumentNullException("effectiveRestriction");

            var person = scheduleDay.Person;
            var scheduleDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
            var personPeriod = person.Period(scheduleDate);
        	var schedulePeriod = person.VirtualSchedulePeriod(scheduleDate);
            var personContract = personPeriod.PersonContract;
            var avgWorkTime = new TimeSpan((long)(person.AverageWorkTimeOfDay(scheduleDate).Ticks  * personContract.PartTimePercentage.Percentage.Value));

			if (!personContract.ContractSchedule.IsWorkday(schedulePeriod.DateOnlyPeriod.StartDate, scheduleDate))
                return new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero)};

            if(!effectiveRestriction.Absence.InContractTime)
                return new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero) };

            IWorkTimeMinMax minMaxLength = new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(avgWorkTime, avgWorkTime) };

            return minMaxLength;
        }

        public static IWorkTimeMinMax GetWorkTime(IScheduleDay scheduleDay)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException("scheduleDay");

            TimeZoneInfo timeZoneInfo = scheduleDay.TimeZone;
            IWorkTimeMinMax minMaxLength = new WorkTimeMinMax();
            IProjectionService projSvc = scheduleDay.ProjectionService();
            var proj = projSvc.CreateProjection();
            TimeSpan contractTime = proj.ContractTime();
            minMaxLength.WorkTimeLimitation = new WorkTimeLimitation(contractTime, contractTime);
            var period = proj.Period();
            if (period != null)
            {
                minMaxLength.StartTimeLimitation =
                    new StartTimeLimitation(period.Value.StartDateTimeLocal(timeZoneInfo).TimeOfDay,
                                            period.Value.StartDateTimeLocal(timeZoneInfo).TimeOfDay);
                minMaxLength.EndTimeLimitation =
                    new EndTimeLimitation(period.Value.EndDateTimeLocal(timeZoneInfo).TimeOfDay,
                                          period.Value.EndDateTimeLocal(timeZoneInfo).TimeOfDay);
            }

            return minMaxLength;
        }
    }
}