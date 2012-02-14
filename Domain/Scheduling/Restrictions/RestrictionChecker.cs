using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    //public enum PermissionState
    //{
    //    Satisfied, Broken, Unspecified,
    //    None
    //}

    public class RestrictionChecker : ICheckerRestriction
    {
        private IScheduleDay _schedulePart;

        public RestrictionChecker(IScheduleDay schedulePart)
        {
            _schedulePart = schedulePart;
        }

        public IScheduleDay ScheduleDay
        {
            get { return _schedulePart;}
            set { _schedulePart = value; }
        }

        public PermissionState CheckAvailability()
        {
            var restriction = (IAvailabilityRestriction)_schedulePart.RestrictionCollection()
                                        .FilterBySpecification(RestrictionMustBe.Availability).FirstOrDefault();
            
            PermissionState permissionState = PermissionState.Unspecified;

            if (restriction!=null)
            {
                //If there is a day off and availability is set to false the restriction is considered Satisfied
                if (_schedulePart.PersonDayOffCollection().Count != 0 && restriction.NotAvailable)
                {
                    return PermissionState.Satisfied;
                }

                IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();
                
                if (visualLayerCollection.HasLayers)
                {
                    if (restriction.NotAvailable)
                        return PermissionState.Broken;
                    permissionState = PermissionState.Satisfied;
                    DateTimePeriod schedulePeriod = (DateTimePeriod)visualLayerCollection.Period();
                    ICccTimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();

                    bool withinStartTimeSpan = isWithinTimeSpan(restriction.StartTimeLimitation.StartTime, restriction.StartTimeLimitation.EndTime, schedulePeriod.StartDateTime, timeZoneInfo);
                    bool withinEndTimeSpan = isWithinTimeSpan(restriction.EndTimeLimitation.StartTime, restriction.EndTimeLimitation.EndTime, schedulePeriod.EndDateTime, timeZoneInfo);

                    if (!withinStartTimeSpan || !withinEndTimeSpan)
                    {
                        permissionState = PermissionState.Broken;
                    }

                    if (!isWorkTimeLengthOk(restriction.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                    {
                        permissionState = PermissionState.Broken;
                    }
                }
            }
            else
            {
                permissionState = PermissionState.None;
            }
            return permissionState;
        }

        public PermissionState CheckRotationDayOff()
        {
            PermissionState permissionState = PermissionState.Unspecified;

            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation != null)
            {
                if (_schedulePart.PersonDayOffCollection().Count == 0 && rotation.DayOffTemplate != null)
                {
                    return PermissionState.Broken;
                }

                //todo: How does the dayoff work?
                foreach (IPersonDayOff dayOff in _schedulePart.PersonDayOffCollection())
                {
                    if (rotation.DayOffTemplate != null)
                    {
                        permissionState = PermissionState.Satisfied;

                        if (!dayOff.CompareToTemplate(rotation.DayOffTemplate))
                        {
                            //Need to do a return here because the visualLayerCollection can be empty 
                            return PermissionState.Broken;
                        }
                    }
                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckRotationShift()
        {
            PermissionState permissionState = PermissionState.Unspecified;

            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation != null)
            {
                var visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

                if (visualLayerCollection.HasLayers && rotation.DayOffTemplate == null)
                {
                    permissionState = PermissionState.Satisfied;

                    var schedulePeriod = (DateTimePeriod)visualLayerCollection.Period();
                    ICccTimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();

                    bool withinStartTimeSpan = isWithinTimeSpan(rotation.StartTimeLimitation.StartTime, rotation.StartTimeLimitation.EndTime, schedulePeriod.StartDateTime, timeZoneInfo);
                    bool withinEndTimeSpan = isWithinTimeSpan(rotation.EndTimeLimitation.StartTime, rotation.EndTimeLimitation.EndTime, schedulePeriod.EndDateTime, timeZoneInfo);

                    if (!withinStartTimeSpan || !withinEndTimeSpan)
                    {
                        permissionState = PermissionState.Broken;
                    }

                    if (!isWorkTimeLengthOk(rotation.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                    {
                        permissionState = PermissionState.Broken;
                    }

                    permissionState = checkRotationShiftCategory(rotation, permissionState);
                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckRotations()
        {
            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                            .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            PermissionState permissionState;

            if (rotation != null)
            {

                permissionState = CheckRotationDayOff();

                if (permissionState == PermissionState.Unspecified || permissionState == PermissionState.Satisfied)
                {
                    var permissionStateShift = CheckRotationShift(); ;

                    if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                        permissionState = permissionStateShift;
                }
                    
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckStudentAvailability()
        {
            PermissionState permissionState = PermissionState.None;
            IEnumerable<IStudentAvailabilityDay> dataRestrictions =
                (from r in _schedulePart.PersistableScheduleDataCollection() where r is IStudentAvailabilityDay select (IStudentAvailabilityDay)r);

            IStudentAvailabilityDay studentAvailabilityDay = dataRestrictions.FirstOrDefault();

            if (studentAvailabilityDay != null && studentAvailabilityDay.RestrictionCollection.Count > 0)
            {
                IStudentAvailabilityRestriction restriction = studentAvailabilityDay.RestrictionCollection[0];
                //If there is a day off and availability is set to false the restriction is considered Satisfied
                if (_schedulePart.PersonDayOffCollection().Count != 0 && studentAvailabilityDay.NotAvailable)
                {
                    return PermissionState.Satisfied;
                }

                IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

                if (visualLayerCollection.HasLayers)
                {
                    if (studentAvailabilityDay.NotAvailable)
                        return PermissionState.Broken;
                    permissionState = PermissionState.Satisfied;
                    DateTimePeriod schedulePeriod = (DateTimePeriod)visualLayerCollection.Period();
                    ICccTimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();

                    bool withinStartTimeSpan = isWithinTimeSpan(restriction.StartTimeLimitation.StartTime, restriction.StartTimeLimitation.EndTime, schedulePeriod.StartDateTime, timeZoneInfo);
                    bool withinEndTimeSpan = isWithinTimeSpan(restriction.EndTimeLimitation.StartTime, restriction.EndTimeLimitation.EndTime, schedulePeriod.EndDateTime, timeZoneInfo);

                    if (!withinStartTimeSpan || !withinEndTimeSpan)
                    {
                        permissionState = PermissionState.Broken;
                    }
                    // we cant set this now anyway
                    //if (!isWorkTimeLengthOk(restriction.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                    //{
                    //    permissionState = PermissionState.Broken;
                    //}
                }
                else
                    return PermissionState.Unspecified;
                
            }

            return permissionState;
        }

        public PermissionState CheckPreferenceDayOff()
        {
            var permissionState = PermissionState.Unspecified;
            var dataRestrictions = (from r in _schedulePart.PersistableScheduleDataCollection() 
                                    where r is IPreferenceDay select (IPreferenceDay)r);

            var preference = (from r in dataRestrictions
                              where r.Restriction is IPreferenceRestriction
                              select r.Restriction).FirstOrDefault();

            if (preference != null)
            {
                if (_schedulePart.PersonDayOffCollection().Count == 0 && preference.DayOffTemplate != null)
                {
                    return PermissionState.Broken;
                }
                //todo: How does the dayoff work? 
                foreach (var dayOff in _schedulePart.PersonDayOffCollection())
                {
                    permissionState = PermissionState.Satisfied;

                    if (preference.DayOffTemplate != null)
                    {
                        if (!dayOff.CompareToTemplate(preference.DayOffTemplate))
                        {
                            //Need to do a return here because the visualLayerCollection can be empty 
                            return PermissionState.Broken;
                        }
                    }
                    else
                        return PermissionState.Broken;
                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        private IPreferenceRestriction RestrictionPreference()
        {
            var dataRestrictions = (from r in _schedulePart.PersistableScheduleDataCollection()
                                    where r is IPreferenceDay
                                    select (IPreferenceDay)r);

            var preference = (from r in dataRestrictions
                              where r.Restriction is IPreferenceRestriction
                              select r.Restriction).FirstOrDefault();

            return preference;
        }

        public PermissionState CheckPreferenceShift()
        {
            var permissionState = PermissionState.Unspecified;

            var preference = RestrictionPreference();

            if (preference != null)
            {
                IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

                if (visualLayerCollection.HasLayers)
                {
                    permissionState = PermissionState.Satisfied;

                    var schedulePeriod = (DateTimePeriod)visualLayerCollection.Period();
                    var timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();

                    var withinStartTimeSpan = isWithinTimeSpan(preference.StartTimeLimitation.StartTime,
                                                                preference.StartTimeLimitation.EndTime,
                                                                schedulePeriod.StartDateTime, timeZoneInfo);

                    var withinEndTimeSpan = isWithinTimeSpan(preference.EndTimeLimitation.StartTime,
                                                              preference.EndTimeLimitation.EndTime,
                                                              schedulePeriod.EndDateTime, timeZoneInfo);

                    if (!withinStartTimeSpan || !withinEndTimeSpan)
                    {
                        permissionState = PermissionState.Broken;
                    }

                    if (!isWorkTimeLengthOk(preference.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                    {
                        permissionState = PermissionState.Broken;
                    }

                    permissionState = checkPreferenceShiftCategory(preference, permissionState);
                    IActivity activity = null;
                    if (preference.ActivityRestrictionCollection.Count > 0)
                    {
                        activity = preference.ActivityRestrictionCollection[0].Activity;
                    }

                    var activities = visualLayerCollection.FilterLayers(activity);
                    foreach (var layer in activities)
                    {
                        if (activity != null)
                        {
                            if (!layer.Payload.Equals(preference.ActivityRestrictionCollection[0].Activity))
                            {
                                permissionState = PermissionState.Broken;
                            }
                        }
                    }


                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckPreference()
        {
            PermissionState permissionState;

            var preference = RestrictionPreference();

            if (preference != null)
            {
                permissionState = CheckPreferenceDayOff();

                if(permissionState != PermissionState.Broken)
                {
                    permissionState = CheckPreferenceAbsence(permissionState);
                }

                if (permissionState == PermissionState.Unspecified || permissionState == PermissionState.Satisfied)
                {
                    var permissionStateShift = CheckPreferenceShift();

                    if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                        permissionState = permissionStateShift;
     
                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckPreferenceMustHave()
        {
            PermissionState permissionState;

            var preference = RestrictionPreference();

            if (preference != null && preference.MustHave)
            {
                permissionState = CheckPreferenceDayOff();

                if (permissionState != PermissionState.Broken)
                {
                    permissionState = CheckPreferenceAbsence(permissionState);
                }

                if (permissionState == PermissionState.Unspecified || permissionState == PermissionState.Satisfied)
                {
                    var permissionStateShift = CheckPreferenceShift();

                    if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                        permissionState = permissionStateShift;

                }
            }
            else
            {
                permissionState = PermissionState.None;
            }

            return permissionState;
        }

        public PermissionState CheckPreferenceAbsence(PermissionState permissionState)
        {
            var preferenceAbsenceChecker = new PreferenceAbsenceChecker(_schedulePart);
            return preferenceAbsenceChecker.CheckPreferenceAbsence(RestrictionPreference(), permissionState);
        }   

        private PermissionState checkPreferenceShiftCategory(IPreferenceRestriction preference, PermissionState permissionState)
        {
            foreach (IPersonAssignment assignment in _schedulePart.PersonAssignmentCollection())
            {
                if (preference.ShiftCategory != null && assignment.MainShift == null)
                {
                    permissionState = PermissionState.Broken;
                }

                if (assignment.MainShift == null) 
                    continue;
                        
                IShiftCategory shiftCategory = assignment.MainShift.ShiftCategory;
                if (preference.ShiftCategory != null)
                {
                    if (!preference.ShiftCategory.Equals(shiftCategory))
                    {
                        permissionState = PermissionState.Broken;
                    }
                }
            }
            return permissionState;
        }

        private PermissionState checkRotationShiftCategory(IRotationRestriction preference, PermissionState permissionState)
        {
            foreach (IPersonAssignment assignment in _schedulePart.PersonAssignmentCollection())
            {
                if (preference.ShiftCategory != null && assignment.MainShift == null)
                {
                    permissionState = PermissionState.Broken;
                }

                if (assignment.MainShift == null)
                    continue;

                IShiftCategory shiftCategory = assignment.MainShift.ShiftCategory;
                if (preference.ShiftCategory != null)
                {
                    if (!preference.ShiftCategory.Equals(shiftCategory))
                    {
                        permissionState = PermissionState.Broken;
                    }
                }
            }
            return permissionState;
        }

        private static bool isWithinTimeSpan(TimeSpan? startTime, TimeSpan? endTime, DateTime timeToCheck, ICccTimeZoneInfo cccTimeZoneInfo)
        {
            bool a = true;
            bool b = true;

            timeToCheck = cccTimeZoneInfo.ConvertTimeFromUtc(timeToCheck, cccTimeZoneInfo);
            if (startTime.HasValue)
            {
                TimeSpan stripped = new TimeSpan(startTime.Value.Hours, startTime.Value.Minutes, 0);
                DateTime minStart = timeToCheck.Date.Add(stripped);
                a = minStart <= timeToCheck;
            }

            if (endTime.HasValue)
            {
                TimeSpan stripped = new TimeSpan(endTime.Value.Hours, endTime.Value.Minutes, 0);
                DateTime maxStart = timeToCheck.Date.Add(stripped);
                b = maxStart >= timeToCheck;
            }

            return a && b;
        }

        private static bool isWorkTimeLengthOk(WorkTimeLimitation limitation, TimeSpan contractLength)
        {
            bool a = true;
            bool b = true;

            if (limitation.StartTime.HasValue)
            {
                if (limitation.StartTime > contractLength)
                {
                    a = false;
                }
            }
            if (limitation.EndTime.HasValue)
            {
                if (limitation.EndTime < contractLength)
                {
                    b = false;
                }
            }

            return a && b;
        }

    }
}
