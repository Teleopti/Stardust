using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class WorkloadAccuracy
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public MethodAccuracy[] Accuracies { get; set; }
	}
}