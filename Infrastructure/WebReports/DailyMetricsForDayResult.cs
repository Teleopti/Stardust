using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DailyMetricsForDayResult
	{
		public int AnsweredCalls { get; set; }
		public TimeSpan AfterCallWorkTimeAverage { get; set; }
		public TimeSpan TalkTimeAverage { get; set; }
		public TimeSpan HandlingTimeAverage { get; set; }
		public Percent ReadyTimePerScheduledReadyTime { get; set; }
		public Percent Adherence { get; set; }
	}
}