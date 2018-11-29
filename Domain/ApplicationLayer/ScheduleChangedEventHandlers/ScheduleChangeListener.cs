using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[Serializable]
	public class ScheduleChangeListener
	{
		public string Name { get; set; }
		public Uri Uri { get; set; }
		public MinMax<int> RelativeDateRange {get; set;}
	}
}