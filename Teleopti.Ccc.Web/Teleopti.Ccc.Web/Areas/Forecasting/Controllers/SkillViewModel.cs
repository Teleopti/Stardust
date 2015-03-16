using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class SkillViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public WorkloadViewModel[] Workloads { get; set; }
	}
}