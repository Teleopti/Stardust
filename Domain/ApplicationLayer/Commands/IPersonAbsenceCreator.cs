using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceCreator
	{
		PersonAbsence Create (IAbsence absence, IScheduleRange scheduleRange, IScheduleDay scheduleDay,
			DateTimePeriod absenceTimePeriod, IPerson person, TrackedCommandInfo command, bool isFullDayAbsence);
	}
}