using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentPreferenceData
	{
		public IShiftCategory ShiftCategory { get; set; }
		public IAbsence Absence { get; set; }
		public IDayOffTemplate DayOffTemplate { get; set; }
		public IActivity Activity { get; set; }

		public TimeSpan? MinStart { get; set; }
		public TimeSpan? MaxStart { get; set; }
		public TimeSpan? MinEnd { get; set; }
		public TimeSpan? MaxEnd { get; set; }
		public TimeSpan? MinLength { get; set; }
		public TimeSpan? MaxLength { get; set; }

		public TimeSpan? MinStartActivity { get; set; }
		public TimeSpan? MaxStartActivity { get; set; }
		public TimeSpan? MinEndActivity { get; set; }
		public TimeSpan? MaxEndActivity { get; set; }
		public TimeSpan? MinLengthActivity { get; set; }
		public TimeSpan? MaxLengthActivity { get; set; }
	}
}
