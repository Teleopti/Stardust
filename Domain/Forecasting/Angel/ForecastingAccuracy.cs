using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class SkillAccuracy
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public WorkloadAccuracy[] Workloads { get; set; }
	}

	public class WorkloadAccuracy
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public MethodAccuracy[] Accuracies { get; set; }
	}

	public class ForecastingAccuracy
	{
		public Guid WorkloadId { get; set; }
		public IList<MethodAccuracy> Accuracies { get; set; }
	}
}