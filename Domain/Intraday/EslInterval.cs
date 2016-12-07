using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class EslInterval
	{
		public DateTime StartTime { get; set; }
		public double ForecastedCalls{ get; set; }
		public double Esl { get; set; }
		public double AnsweredCallsWithinServiceLevel => ForecastedCalls * Esl;
	}
}