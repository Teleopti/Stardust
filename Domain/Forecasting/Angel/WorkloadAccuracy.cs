using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class WorkloadAccuracy
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public MethodAccuracy[] Accuracies { get; set; }
	}
}