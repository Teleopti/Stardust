using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatUsageForInterval
	{
		DateTime IntervalStart { get; set; }
		DateTime IntervalEnd { get; set; }
		int SeatUsage { get; set; }
	}
}
