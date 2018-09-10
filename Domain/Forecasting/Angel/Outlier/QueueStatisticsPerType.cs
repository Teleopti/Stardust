using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public class QueueStatisticsPerType
	{
		public Dictionary<DateOnly, double> Tasks { get; set; }
		public Dictionary<DateOnly, double> TaskTime { get; set; }
		public Dictionary<DateOnly, double> AfterTaskTime { get; set; }
	}
}