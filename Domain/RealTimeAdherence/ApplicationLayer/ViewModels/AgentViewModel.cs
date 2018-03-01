using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class AgentViewModel
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }
		public string TeamId { get; set; }
		public string TeamName { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }
	}

}