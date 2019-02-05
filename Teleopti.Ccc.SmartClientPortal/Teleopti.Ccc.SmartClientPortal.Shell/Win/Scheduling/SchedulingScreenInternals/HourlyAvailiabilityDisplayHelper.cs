using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public class HourlyAvailiabilityDisplayHelper
	{
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

	}
}