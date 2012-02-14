using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
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
        private readonly IRuleSetProjectionService _projectionService;

        public MinMaxWorkTimeChecker(IRuleSetProjectionService projectionService)
        {
            if (projectionService == null)
                throw new ArgumentNullException("projectionService");

            _projectionService = projectionService;
        }

        public IWorkTimeMinMax MinMaxWorkTime(IScheduleDay scheduleDay, IRuleSetBag ruleSetBag, IEffectiveRestriction effectiveRestriction)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException("scheduleDay");

            if (ruleSetBag == null)
                throw new ArgumentNullException("ruleSetBag");

            var significant = scheduleDay.SignificantPart();

            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                return new WorkTimeMinMax();

            if (significant == SchedulePartView.MainShift || significant == SchedulePartView.FullDayAbsence)
            {
                return GetWorkTime(scheduleDay);
            }

            if (effectiveRestriction != null && effectiveRestriction.Absence != null)
            {
                return GetWorkTimeAbsencePreference(scheduleDay, effectiveRestriction);
            }

            effectiveRestriction = GetEffectiveRestrictionForMeeting(scheduleDay, effectiveRestriction);
            effectiveRestriction = GetEffectiveRestrictionForPersonalShift(scheduleDay, effectiveRestriction);

            var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            return ruleSetBag.MinMaxWorkTime(_projectionService, dateOnly, effectiveRestriction);
           
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
            var personContract = personPeriod.PersonContract;
            var avgWorkTime = new TimeSpan((long)(personContract.Contract.WorkTime.AvgWorkTimePerDay.Ticks * personContract.PartTimePercentage.Percentage.Value));

            if (!personContract.ContractSchedule.IsWorkday(personPeriod.StartDate, scheduleDate))
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

            ICccTimeZoneInfo timeZoneInfo = scheduleDay.TimeZone;
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

        public static IEffectiveRestriction GetEffectiveRestrictionForPersonalShift(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException("scheduleDay");

            if (effectiveRestriction == null)
                return null;

            if (scheduleDay.PersonAssignmentCollection().IsEmpty())
                return effectiveRestriction;

            //inte på parten här??????????
            IPerson person = scheduleDay.Person;
            ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

            foreach (IPersonAssignment assignment in scheduleDay.PersonAssignmentCollection())
            {
                foreach (IPersonalShift shift in assignment.PersonalShiftCollection)
                {
                    var personalShiftPeriod = shift.LayerCollection.Period();
                    if (!personalShiftPeriod.HasValue) continue;
                    var personalShiftRestriction = new EffectiveRestriction(
                        new StartTimeLimitation(null, personalShiftPeriod.Value.TimePeriod(timeZoneInfo).StartTime),
                        new EndTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).EndTime, null),
                        new WorkTimeLimitation(personalShiftPeriod.Value.TimePeriod(timeZoneInfo).SpanningTime(), null),
                        null, null, null, new List<IActivityRestriction>());
                    effectiveRestriction = effectiveRestriction.Combine(personalShiftRestriction);
                }
            }
            return effectiveRestriction;
        }


        public static IEffectiveRestriction GetEffectiveRestrictionForMeeting(IScheduleDay scheduleDay, IEffectiveRestriction effectiveRestriction)
        {
            if (scheduleDay == null)
                throw new ArgumentNullException("scheduleDay");

            if (effectiveRestriction == null)
                return null;

            if (scheduleDay.PersonMeetingCollection().IsEmpty())
                return effectiveRestriction;

            //inte på parten här??????????
            IPerson person = scheduleDay.Person;
            ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();

            foreach (IPersonMeeting meeting in scheduleDay.PersonMeetingCollection())
            {
                DateTimePeriod personalShiftPeriod = meeting.Period;
                var personalShiftRestriction = new EffectiveRestriction(
                        new StartTimeLimitation(null, personalShiftPeriod.TimePeriod(timeZoneInfo).StartTime),
                        new EndTimeLimitation(personalShiftPeriod.TimePeriod(timeZoneInfo).EndTime, null),
                        new WorkTimeLimitation(personalShiftPeriod.TimePeriod(timeZoneInfo).SpanningTime(), null), null,
                        null, null, new List<IActivityRestriction>());
                effectiveRestriction = effectiveRestriction.Combine(personalShiftRestriction);
            }
            return effectiveRestriction;
        }
    }
}