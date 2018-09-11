
using System;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class IntradayForecastInterval
	{
		public Guid SkillId { get; set; }
		public DateTime StartTime { get; set; }
		public double Agents { get; set; }
		public double Calls { get; set; }
		public double AverageHandleTime { get; set; }
	}
}
