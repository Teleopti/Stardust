using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeniorityWorkDayRanks : IAggregateRoot, IBelongsToBusinessUnit, IChangeInfo
	{
		int Monday { get; set; }
		int Tuesday { get; set; }
		int Wednesday { get; set; }
		int Thursday { get; set; }
		int Friday { get; set; }
		int Saturday { get; set; }
		int Sunday { get; set; }

		IList<ISeniorityWorkDay> SeniorityWorkDays();
	}
}
