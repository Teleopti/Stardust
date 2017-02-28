using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeniorityWorkDay
	{
		DayOfWeek DayOfWeek { get; set; }
		int Rank { get; set; }
		string DayOfWeekName { get; }
	}
}
