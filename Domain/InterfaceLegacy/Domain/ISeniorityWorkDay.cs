using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeniorityWorkDay
	{
		DayOfWeek DayOfWeek { get; set; }
		int Rank { get; set; }
		string DayOfWeekName { get; }
	}
}
