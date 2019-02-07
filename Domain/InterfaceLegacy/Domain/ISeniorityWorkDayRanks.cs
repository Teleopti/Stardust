using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeniorityWorkDayRanks : IAggregateRoot, IFilterOnBusinessUnit, IChangeInfo
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
