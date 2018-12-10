using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		public double? OverrideTasks { get; private set; }
		public TimeSpan? OverrideAverageTaskTime { get; set; }
		public TimeSpan? OverrideAverageAfterTaskTime { get; set; }

		public OpenForWork OpenForWork { get; private set; }
	}
}