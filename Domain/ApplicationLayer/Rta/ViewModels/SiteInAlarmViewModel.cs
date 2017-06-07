using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteInAlarmViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int AgentsCount { get; set; }
		public int InAlarmCount { get; set; }
		public string Color { get; set; }
	}
}