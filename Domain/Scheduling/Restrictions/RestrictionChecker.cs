using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class RestrictionChecker : ICheckerRestriction
    {
        private IScheduleDay _schedulePart;

        public RestrictionChecker()
        { }

        public RestrictionChecker(IScheduleDay schedulePart)
        {
            _schedulePart = schedulePart;
        }

        public IScheduleDay ScheduleDay
        {
            get { return _schedulePart; }
            set { _schedulePart = value; }
        }

    	public bool MustHavePreference
    	{
    		get; set; 
		}

        public PermissionState CheckAvailability()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var restriction = (IAvailabilityRestriction)_schedulePart.RestrictionCollection()
                                        .FilterBySpecification(RestrictionMustBe.Availability).FirstOrDefault();

            if (restriction == null)
                return PermissionState.None;
        	SchedulePartView significant = _schedulePart.SignificantPart();

            //If there is a day off and availability is set to false the restriction is considered Satisfied
            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.FullDayAbsence || significant == SchedulePartView.ContractDayOff)
            {
                return PermissionState.Satisfied;
            }

            PermissionState permissionState = PermissionState.Unspecified;
            IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers)
            {
                if (restriction.NotAvailable)
                    return PermissionState.Broken;
                permissionState = PermissionState.Satisfied;
                DateTimePeriod schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
                TimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();
            	var localTimePeriod = schedulePeriod.TimePeriod(timeZoneInfo);

                bool withinStartTimeSpan = isWithinTimeSpan(restriction.StartTimeLimitation.StartTime, restriction.StartTimeLimitation.EndTime, localTimePeriod.StartTime);
                bool withinEndTimeSpan = isWithinTimeSpan(restriction.EndTimeLimitation.StartTime, restriction.EndTimeLimitation.EndTime, localTimePeriod.EndTime);

                if (!withinStartTimeSpan || !withinEndTimeSpan)
                {
                    permissionState = PermissionState.Broken;
                }

                if (!isWorkTimeLengthOk(restriction.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                {
                    permissionState = PermissionState.Broken;
                }
            }

            return permissionState;
        }

        public PermissionState CheckRotationDayOff()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null)
                return PermissionState.None;

            PermissionState permissionState = PermissionState.Unspecified;

            if (!_schedulePart.HasDayOff() && rotation.DayOffTemplate != null)
            {
                return PermissionState.Broken;
            }

	        var ass = _schedulePart.PersonAssignment();
					if (ass != null)
					{
						if (rotation.DayOffTemplate != null)
						{
							permissionState = PermissionState.Satisfied;

							if (!ass.AssignedWithDayOff(rotation.DayOffTemplate))
							{
								//Need to do a return here because the visualLayerCollection can be empty 
								return PermissionState.Broken;
							}
						}
					}

            return permissionState;
        }

        public PermissionState CheckRotationShift()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null)
                return PermissionState.None;


            PermissionState permissionState = PermissionState.Unspecified;
            
            if(_schedulePart.HasDayOff())
            {
                if(rotation.DayOffTemplate == null)
                    return PermissionState.Broken;
            }

            var visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers && rotation.DayOffTemplate == null)
            {
                permissionState = PermissionState.Satisfied;

                var schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
                TimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();
            	var localTimePeriod = schedulePeriod.TimePeriod(timeZoneInfo);

                bool withinStartTimeSpan = isWithinTimeSpan(rotation.StartTimeLimitation.StartTime, rotation.StartTimeLimitation.EndTime, localTimePeriod.StartTime);
                bool withinEndTimeSpan = isWithinTimeSpan(rotation.EndTimeLimitation.StartTime, rotation.EndTimeLimitation.EndTime, localTimePeriod.EndTime);

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

            return permissionState;
        }

        public PermissionState CheckRotations()
        {

            if (_schedulePart == null)
                return PermissionState.None;

            var rotation = (IRotationRestriction)_schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null || !rotation.IsRestriction())
                return PermissionState.None;

            PermissionState permissionState = CheckRotationDayOff();

            if (permissionState == PermissionState.Unspecified
                || permissionState == PermissionState.Satisfied)
            {
                var permissionStateShift = CheckRotationShift();

                if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                    permissionState = permissionStateShift;
            }


            return permissionState;
        }

        public PermissionState CheckStudentAvailability()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            PermissionState permissionState = PermissionState.None;

            IEnumerable<IStudentAvailabilityDay> dataRestrictions =
                (from r in _schedulePart.PersistableScheduleDataCollection() where r is IStudentAvailabilityDay select (IStudentAvailabilityDay)r);

            IStudentAvailabilityDay studentAvailabilityDay = dataRestrictions.FirstOrDefault();

            if (studentAvailabilityDay != null
                && studentAvailabilityDay.RestrictionCollection.Count > 0)
            {
                IStudentAvailabilityRestriction restriction = studentAvailabilityDay.RestrictionCollection[0];
                //If there is a day off and availability is set to false the restriction is considered Satisfied
                if (_schedulePart.HasDayOff() && studentAvailabilityDay.NotAvailable)
                {
                    return PermissionState.Satisfied;
                }

                IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

                if (visualLayerCollection.HasLayers)
                {
                    if (studentAvailabilityDay.NotAvailable)
                        return PermissionState.Broken;
                    permissionState = PermissionState.Satisfied;
                    DateTimePeriod schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
                    TimeZoneInfo timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();

                	var localTimePeriod = schedulePeriod.TimePeriod(timeZoneInfo);
                    bool withinStartTimeSpan = isWithinTimeSpan(restriction.StartTimeLimitation.StartTime, restriction.StartTimeLimitation.EndTime, localTimePeriod.StartTime);
                    bool withinEndTimeSpan = isWithinTimeSpan(restriction.EndTimeLimitation.StartTime, restriction.EndTimeLimitation.EndTime, localTimePeriod.EndTime);

                    if (!withinStartTimeSpan || !withinEndTimeSpan)
                    {
                        permissionState = PermissionState.Broken;
                    }
                }
                else
                    return PermissionState.Unspecified;

            }

            return permissionState;
        }

        public PermissionState CheckPreferenceDayOff()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var permissionState = PermissionState.Unspecified;
            var dataRestrictions = (from r in _schedulePart.PersistableScheduleDataCollection()
                                    where r is IPreferenceDay
                                    select (IPreferenceDay)r);

            var preference = (from r in dataRestrictions
                              where r.Restriction != null
                              select r.Restriction).FirstOrDefault();

            if (preference == null)
                return PermissionState.None;

			if (_schedulePart.SignificantPart() == SchedulePartView.MainShift && preference.DayOffTemplate != null)
				return PermissionState.Broken;

			if (!_schedulePart.IsScheduled() && preference.DayOffTemplate != null)
				return PermissionState.Unspecified;

	        var ass = _schedulePart.PersonAssignment();
					if (ass != null)
					{
						var dayOff = ass.DayOff();
						if (dayOff != null)
						{
							permissionState = PermissionState.Satisfied;

							if (preference.DayOffTemplate != null)
							{
								if (!ass.AssignedWithDayOff(preference.DayOffTemplate))
								{
									//Need to do a return here because the visualLayerCollection can be empty 
									return PermissionState.Broken;
								}
							}
							else
								return PermissionState.Broken;
						}
					}
           return permissionState;
        }

        private IPreferenceRestriction RestrictionPreference()
        {
            var dataRestrictions = (from r in _schedulePart.PersistableScheduleDataCollection()
                                    where r is IPreferenceDay
                                    select (IPreferenceDay)r);

            var preference = (from r in dataRestrictions
                              where r.Restriction != null
                              select r.Restriction).FirstOrDefault();

            return preference;
        }

        public PermissionState CheckPreferenceShift()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var permissionState = PermissionState.Unspecified;

            var preference = RestrictionPreference();

            if (preference == null)
                return PermissionState.None;

            IVisualLayerCollection visualLayerCollection = _schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers)
            {
                permissionState = PermissionState.Satisfied;

                var schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
                var timeZoneInfo = _schedulePart.Person.PermissionInformation.DefaultTimeZone();
            	var localTimePeriod = schedulePeriod.TimePeriod(timeZoneInfo);

                var withinStartTimeSpan = isWithinTimeSpan(preference.StartTimeLimitation.StartTime,
                                                            preference.StartTimeLimitation.EndTime,
															localTimePeriod.StartTime);

                var withinEndTimeSpan = isWithinTimeSpan(preference.EndTimeLimitation.StartTime,
                                                          preference.EndTimeLimitation.EndTime,
                                                          localTimePeriod.EndTime);

                if (!withinStartTimeSpan || !withinEndTimeSpan)
                {
                    permissionState = PermissionState.Broken;
                }

                if (!isWorkTimeLengthOk(preference.WorkTimeLimitation, visualLayerCollection.ContractTime()))
                {
                    permissionState = PermissionState.Broken;
                }

                permissionState = checkPreferenceShiftCategory(preference, permissionState);


				if (preference.ActivityRestrictionCollection.Count > 0)
				{
					var activityRestriction = preference.ActivityRestrictionCollection[0];
					var activities = visualLayerCollection.FilterLayers(activityRestriction.Activity);

					foreach (var layer in activities)
					{
						var withinActivityStartTimeSpan = isWithinTimeSpan(activityRestriction.StartTimeLimitation.StartTime, activityRestriction.StartTimeLimitation.EndTime, layer.Period.TimePeriod(timeZoneInfo).StartTime);
						var withinActivityEndTimeSpan = isWithinTimeSpan(activityRestriction.EndTimeLimitation.StartTime, activityRestriction.EndTimeLimitation.EndTime, layer.Period.TimePeriod(timeZoneInfo).EndTime);

						if (!withinActivityStartTimeSpan || !withinActivityEndTimeSpan)
						{
							permissionState = PermissionState.Broken;
						}

						if (!isWorkTimeLengthOk(activityRestriction.WorkTimeLimitation, layer.Period.TimePeriod(timeZoneInfo).SpanningTime()))
						{
							permissionState = PermissionState.Broken;
						}
					}

					if (!activities.Any())
						permissionState = PermissionState.Broken;

				}
            }

            return permissionState;
        }

        public PermissionState CheckPreference()
        {
            if (_schedulePart == null)
                return PermissionState.None;

            var preference = RestrictionPreference();

            if (preference == null)
                return PermissionState.None;

            MustHavePreference = preference.MustHave;

            PermissionState permissionState = CheckPreferenceDayOff();

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

            return permissionState;
        }

        public PermissionState CheckPreferenceMustHave()
        {
            if (_schedulePart == null)
                return PermissionState.None;

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
					var assignment = _schedulePart.PersonAssignment();
					if (assignment != null)
					{
						IShiftCategory shiftCategory = assignment.ShiftCategory;
						if (preference.ShiftCategory != null && shiftCategory == null)
						{
							permissionState = PermissionState.Broken;
						}

						if (shiftCategory == null)
							return permissionState;

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
	        var assignment = _schedulePart.PersonAssignment();
					if (assignment != null)
					{
						IShiftCategory shiftCategory = assignment.ShiftCategory;
						if (preference.ShiftCategory != null && shiftCategory == null)
						{
							permissionState = PermissionState.Broken;
						}

						if (shiftCategory == null)
							return permissionState;

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

	    private static bool isWithinTimeSpan(TimeSpan? startTime, TimeSpan? endTime, TimeSpan scheduleTime)
        {
            bool minBoundaryFulfilled = true;
            bool maxBoundaryFulfilled = true;

            if (startTime.HasValue)
            {
                minBoundaryFulfilled = startTime <= scheduleTime;
            }

            if (endTime.HasValue)
            {
                maxBoundaryFulfilled = endTime >= scheduleTime;
            }

            return minBoundaryFulfilled && maxBoundaryFulfilled;
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
