using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class TeamOutOfAdherence
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int AgentsCount { get; set; }
		public Guid SiteId { get; set; }
		public int InAlarmCount { get; set; }
		public string Color { get; set; }
	}
}