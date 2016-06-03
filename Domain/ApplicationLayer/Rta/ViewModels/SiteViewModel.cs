using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int NumberOfAgents { get; set; }
	}
}