using System;
using System.Collections.Generic;
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
		public double? OverrideTasks { get; private set; }
		public TimeSpan? OverrideAverageTaskTime { get; set; }
		public TimeSpan? OverrideAverageAfterTaskTime { get; set; }
		public void SetOverrideTasks(double? task, IEnumerable<ITaskOwner> intradayPattern)
		{
			throw new NotImplementedException();
		}

		public OpenForWork OpenForWork { get; private set; }
	}
}