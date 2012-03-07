using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IHasDayOffUnderFullDayAbsence
	{
		bool HasDayOff(IScheduleDay scheduleDay);
	}

	public class HasDayOffUnderFullDayAbsence : IHasDayOffUnderFullDayAbsence
	{
		public bool HasDayOff(IScheduleDay scheduleDay)
		{
			if (scheduleDay == null)
				return false;

			if (scheduleDay.SignificantPartForDisplay() != SchedulePartView.FullDayAbsence)
				return false;

			var dayOffCollection = scheduleDay.PersonDayOffCollection();
			if (dayOffCollection != null && dayOffCollection.Any())
				return true;

			if (new HasDayOffDefinition(scheduleDay).IsDayOff())
				return true;

			return false;
		}
	}
}