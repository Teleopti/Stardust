using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class SkillAccuracy
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public WorkloadAccuracy[] Workloads { get; set; }
	}
}