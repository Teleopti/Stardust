using System;
using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteCardViewModel
	{
		public int TotalAgentsInAlarm { get; set; }
		public IEnumerable<SiteViewModel> Sites { get; set; }
	}

	public class SiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int AgentsCount { get; set; }
		public int InAlarmCount { get; set; }
		public string Color { get; set; }
	}
}