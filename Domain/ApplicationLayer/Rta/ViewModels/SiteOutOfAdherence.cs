using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteOutOfAdherence
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int NumberOfAgents { get; set; }
		// Remove me
		public IEnumerable<SiteOpenHourViewModel> OpenHours { get; set; }
		public int OutOfAdherence { get; set; }
		public string Color { get; set; }
	}
}