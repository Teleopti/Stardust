using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class TimeFilterInfo
	{
		public IEnumerable<DateTimePeriod> StartTimes { get; set; }
		public IEnumerable<DateTimePeriod> EndTimes { get; set; } 
		public bool IsDayOff { get; set; }
	}
}