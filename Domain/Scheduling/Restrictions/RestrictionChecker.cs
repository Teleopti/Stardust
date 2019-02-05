using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class RestrictionChecker : ICheckerRestriction
    {
        public PermissionState CheckAvailability(IScheduleDay schedulePart)
        {
            if (schedulePart == null)
                return PermissionState.None;

			var restriction = (IAvailabilityRestriction)schedulePart.RestrictionCollection()
                                        .FilterBySpecification(RestrictionMustBe.Availability).FirstOrDefault();

            if (restriction == null)
                return PermissionState.None;
			SchedulePartView significant = schedulePart.SignificantPart();

            //If there is a day off and availability is set to false the restriction is considered Satisfied
            if (significant == SchedulePartView.DayOff || significant == SchedulePartView.FullDayAbsence || significant == SchedulePartView.ContractDayOff)
            {
                return PermissionState.Satisfied;
            }

            PermissionState permissionState = PermissionState.Unspecified;
			IVisualLayerCollection visualLayerCollection = schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers)
            {
                if (restriction.NotAvailable)
                    return PermissionState.Broken;
                permissionState = PermissionState.Satisfied;
                DateTimePeriod schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
				TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
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

		public PermissionState CheckRotationDayOff(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

			var rotation = (IRotationRestriction)schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null)
                return PermissionState.None;

            PermissionState permissionState = PermissionState.Unspecified;

			if (!schedulePart.HasDayOff() && rotation.DayOffTemplate != null)
            {
                return PermissionState.Broken;
            }

			var ass = schedulePart.PersonAssignment();
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

		public PermissionState CheckRotationShift(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

			var rotation = (IRotationRestriction)schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null)
                return PermissionState.None;


            PermissionState permissionState = PermissionState.Unspecified;

			if (schedulePart.HasDayOff())
            {
                if(rotation.DayOffTemplate == null)
                    return PermissionState.Broken;
            }

			var visualLayerCollection = schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers && rotation.DayOffTemplate == null)
            {
                permissionState = PermissionState.Satisfied;

                var schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
                TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
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

				permissionState = checkRotationShiftCategory(rotation, permissionState, schedulePart);
            }

            return permissionState;
        }

		public PermissionState CheckRotations(IScheduleDay schedulePart)
        {

            if (schedulePart == null)
                return PermissionState.None;

			var rotation = (IRotationRestriction)schedulePart.RestrictionCollection()
                           .FilterBySpecification(RestrictionMustBe.Rotation).FirstOrDefault();

            if (rotation == null || !rotation.IsRestriction())
                return PermissionState.None;

			PermissionState permissionState = CheckRotationDayOff(schedulePart);

            if (permissionState == PermissionState.Unspecified
                || permissionState == PermissionState.Satisfied)
            {
				var permissionStateShift = CheckRotationShift(schedulePart);

                if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                    permissionState = permissionStateShift;
            }


            return permissionState;
        }

		public bool IsAnyAvailabilityLeftToUse(IScheduleDay schedulePart)
		{
			if (schedulePart == null)
				return false;

			var dataRestrictions = schedulePart.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>();
			var studentAvailabilityDay = dataRestrictions.FirstOrDefault();
			if (studentAvailabilityDay.NotAvailable)
				return false;

			IVisualLayerCollection visualLayerCollection = schedulePart.ProjectionService().CreateProjection();
			if (!visualLayerCollection.HasLayers)
				return false;

			DateTimePeriod schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
			TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
			var localTimePeriod = schedulePeriod.TimePeriod(timeZoneInfo);
			IStudentAvailabilityRestriction restriction = studentAvailabilityDay.RestrictionCollection[0];
			bool withinStartTimeSpan = isWellWithinTimeSpan(restriction.StartTimeLimitation.StartTime, restriction.StartTimeLimitation.EndTime, localTimePeriod.StartTime);
			bool withinEndTimeSpan = isWellWithinTimeSpan(restriction.EndTimeLimitation.StartTime, restriction.EndTimeLimitation.EndTime, localTimePeriod.EndTime);

			if (withinStartTimeSpan || withinEndTimeSpan)
			{
				return true;
			}

			return false;
		}

		public PermissionState CheckStudentAvailability(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

            var dataRestrictions = schedulePart.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>();
            var studentAvailabilityDay = dataRestrictions.FirstOrDefault();
			
			if(studentAvailabilityDay == null && !schedulePart.PersonAssignment(true).ShiftLayers.IsEmpty())
				return PermissionState.Broken;
			
			var permissionState = PermissionState.None;

            if (studentAvailabilityDay != null
                && studentAvailabilityDay.RestrictionCollection.Count > 0)
            {
                IStudentAvailabilityRestriction restriction = studentAvailabilityDay.RestrictionCollection[0];
                //If there is a day off and availability is set to false the restriction is considered Satisfied
				if (schedulePart.HasDayOff() && studentAvailabilityDay.NotAvailable)
                {
                    return PermissionState.Satisfied;
                }

				IVisualLayerCollection visualLayerCollection = schedulePart.ProjectionService().CreateProjection();

                if (visualLayerCollection.HasLayers)
                {
                    if (studentAvailabilityDay.NotAvailable)
                        return PermissionState.Broken;
                    permissionState = PermissionState.Satisfied;
                    DateTimePeriod schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
					TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();

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

		public PermissionState CheckPreferenceDayOff(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

            var permissionState = PermissionState.Unspecified;
			var preference = restrictionPreference(schedulePart);

            if (preference == null)
                return PermissionState.None;

			if (schedulePart.SignificantPart() == SchedulePartView.MainShift && preference.DayOffTemplate != null)
				return PermissionState.Broken;

			if (!schedulePart.IsScheduled() && preference.DayOffTemplate != null)
				return PermissionState.Broken;

			permissionState = checkPrefrenceDayOffAssignedWithDayOff(schedulePart, permissionState, preference);

	        return permissionState;
        }

		public PermissionState CheckPreferenceDayOffForDisplay(IScheduleDay schedulePart)
		{
			if (schedulePart == null)
				return PermissionState.None;

			var permissionState = PermissionState.Unspecified;
			var preference = restrictionPreference(schedulePart);

			if (preference == null)
				return PermissionState.None;

			if (schedulePart.SignificantPart() == SchedulePartView.MainShift && preference.DayOffTemplate != null)
				return PermissionState.Broken;

			if (!schedulePart.IsScheduled() && preference.DayOffTemplate != null)
				return PermissionState.Unspecified;

			permissionState = checkPrefrenceDayOffAssignedWithDayOff(schedulePart, permissionState, preference);

			return permissionState;
		}

		private static PermissionState checkPrefrenceDayOffAssignedWithDayOff(IScheduleDay schedulePart, PermissionState permissionState, IPreferenceRestriction preference)
		{
			var ass = schedulePart.PersonAssignment();
			var dayOff = ass?.DayOff();
			if (dayOff == null) return permissionState;
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

			return permissionState;
		}

		private static IPreferenceRestriction restrictionPreference(IScheduleDay schedulePart)
        {
			var preferenceDay = schedulePart.PreferenceDay();
			return preferenceDay?.Restriction;
		}

		public PermissionState CheckPreferenceShift(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

            var permissionState = PermissionState.Unspecified;

			var preference = restrictionPreference(schedulePart);

            if (preference == null)
                return PermissionState.None;

			IVisualLayerCollection visualLayerCollection = schedulePart.ProjectionService().CreateProjection();

            if (visualLayerCollection.HasLayers)
            {
                permissionState = PermissionState.Satisfied;

                var schedulePeriod = visualLayerCollection.Period().GetValueOrDefault();
				var timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
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

				permissionState = checkPreferenceShiftCategory(preference, permissionState, schedulePart);


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

		public PermissionState CheckPreference(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

			var preference = restrictionPreference(schedulePart);

            if (preference == null)
                return PermissionState.None;

			PermissionState permissionState = CheckPreferenceDayOff(schedulePart);

            if (permissionState != PermissionState.Broken)
            {
				permissionState = CheckPreferenceAbsence(permissionState, schedulePart);
            }

            if (permissionState == PermissionState.Unspecified || permissionState == PermissionState.Satisfied)
            {
				var permissionStateShift = CheckPreferenceShift(schedulePart);

                if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied)
                    permissionState = permissionStateShift;

            }

            return permissionState;
        }

	    public bool HaveMustHavePreference(IScheduleDay schedulePart)
	    {
			if (schedulePart == null)
				return false;

			var preference = restrictionPreference(schedulePart);

			if (preference == null)
				return false;

			return preference.MustHave;
		}

		public PermissionState CheckPreferenceMustHave(IScheduleDay schedulePart)
        {
			if (schedulePart == null)
                return PermissionState.None;

            PermissionState permissionState;

			var preference = restrictionPreference(schedulePart);

            if (preference != null && preference.MustHave)
            {
				permissionState = CheckPreferenceDayOff(schedulePart);

                if (permissionState != PermissionState.Broken)
                {
					permissionState = CheckPreferenceAbsence(permissionState, schedulePart);
                }

                if (permissionState == PermissionState.Unspecified || permissionState == PermissionState.Satisfied)
                {
					var permissionStateShift = CheckPreferenceShift(schedulePart);

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

		public PermissionState CheckPreferenceAbsence(PermissionState permissionState, IScheduleDay schedulePart)
        {
			var preferenceAbsenceChecker = new PreferenceAbsenceChecker(schedulePart);
			return preferenceAbsenceChecker.CheckPreferenceAbsence(restrictionPreference(schedulePart), permissionState);
        }

		private PermissionState checkPreferenceShiftCategory(IPreferenceRestriction preference, PermissionState permissionState, IScheduleDay schedulePart)
        {
					var assignment = schedulePart.PersonAssignment();
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

		private PermissionState checkRotationShiftCategory(IRotationRestriction preference, PermissionState permissionState, IScheduleDay schedulePart)
        {
			var assignment = schedulePart.PersonAssignment();
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

		private static bool isWellWithinTimeSpan(TimeSpan? startTime, TimeSpan? endTime, TimeSpan scheduleTime)
		{
			if (startTime.HasValue)
			{
				var within = startTime < scheduleTime;
				if (within)
					return true;
			}

			if (endTime.HasValue)
			{
				var within = endTime > scheduleTime;
				if (within)
					return true;
			}

			return false;
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

	        if (limitation.StartTime > contractLength)
	        {
		        a = false;
	        }
	        if (limitation.EndTime < contractLength)
	        {
		        b = false;
	        }

	        return a && b;
        }
    }
}
