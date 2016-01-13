using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class SkillAccuracy
	{
		public Guid Id { get; set; }
		public WorkloadAccuracy[] Workloads { get; set; }
	}
}