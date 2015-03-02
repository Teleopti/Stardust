using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class ForecastingTarget : IForecastingTarget
	{
		public ForecastingTarget(DateOnly day, OpenForWork openForWork)
		{
			CurrentDate = day;
			OpenForWork = openForWork;
		}

		public DateOnly CurrentDate { get; private set; }
		public double Tasks { get; set; }
		public TimeSpan AverageAfterTaskTime { get; set; }
		public TimeSpan AverageTaskTime { get; set; }
		public OpenForWork OpenForWork { get; private set; }
	}
}