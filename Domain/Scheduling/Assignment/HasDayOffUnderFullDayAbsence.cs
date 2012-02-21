using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IHasDayOffUnderFullDayAbsence
	{
		bool HasDayOff();
	}
	
	public class HasDayOffUnderFullDayAbsence : IHasDayOffUnderFullDayAbsence
	{
		private readonly IScheduleDay _scheduleDay;
		private SchedulePartView _significantPart;
		private ReadOnlyCollection<IPersonDayOff> _dayOffCollection;
		private HasDayOffDefinition _hasDayOff;


		public HasDayOffUnderFullDayAbsence(IScheduleDay scheduleDay)
		{
			_scheduleDay = scheduleDay;
			_significantPart = _scheduleDay.SignificantPartForDisplay();
			_dayOffCollection = _scheduleDay.PersonDayOffCollection();
			_hasDayOff = new HasDayOffDefinition(_scheduleDay);
		}

		public bool HasDayOff()
		{
			return (((_dayOffCollection != null && _dayOffCollection.ToList().Count > 0) || _hasDayOff.IsDayOff()) &&
			        _significantPart == SchedulePartView.FullDayAbsence);
		}
	}
}