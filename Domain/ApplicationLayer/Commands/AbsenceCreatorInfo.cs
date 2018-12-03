using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AbsenceCreatorInfo
	{
		public IPerson Person { get; set; }
		public IAbsence Absence { get; set; }
		public IScheduleDay ScheduleDay { get; set; }
		public IScheduleRange ScheduleRange { get; set; }
		public IPersonRequest PersonRequest{ get; set; }
		public DateTimePeriod AbsenceTimePeriod { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}