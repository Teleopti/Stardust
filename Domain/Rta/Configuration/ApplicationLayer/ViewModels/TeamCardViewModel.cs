using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class TeamCardViewModel
	{
		public Guid Id { get; set; }
		public Guid SiteId { get; set; }
		public string Name { get; set; }
		public int AgentsCount { get; set; }
		public int InAlarmCount { get; set; }
		public string Color { get; set; }
	}
}