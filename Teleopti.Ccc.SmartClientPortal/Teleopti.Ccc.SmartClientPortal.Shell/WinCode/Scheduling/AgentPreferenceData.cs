using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IAgentPreferenceData
	{
		IShiftCategory ShiftCategory { get; set; }
		IAbsence Absence { get; set; }
		IDayOffTemplate DayOffTemplate { get; set; }
		IActivity Activity { get; set; }
		TimeSpan? MinStart { get; set; }
		TimeSpan? MaxStart { get; set; }
		TimeSpan? MinEnd { get; set; }
		TimeSpan? MaxEnd { get; set; }
		TimeSpan? MinLength { get; set; }
		TimeSpan? MaxLength { get; set; }
		TimeSpan? MinStartActivity { get; set; }
		TimeSpan? MaxStartActivity { get; set; }
		TimeSpan? MinEndActivity { get; set; }
		TimeSpan? MaxEndActivity { get; set; }
		TimeSpan? MinLengthActivity { get; set; }
		TimeSpan? MaxLengthActivity { get; set; }
		bool MustHave { get; set; }
	}

	public class AgentPreferenceData : IAgentPreferenceData
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

		public bool MustHave { get; set; }
	}
}
