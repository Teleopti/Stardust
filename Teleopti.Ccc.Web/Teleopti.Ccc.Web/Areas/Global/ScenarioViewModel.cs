using System;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ScenarioViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool DefaultScenario { get; set; }
	}
}