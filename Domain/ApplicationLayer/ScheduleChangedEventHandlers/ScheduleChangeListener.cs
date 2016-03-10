using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangeListener
	{
		public string Name { get; set; }
		public Uri Uri { get; set; }
		public MinMax<int> RelativeDateRange {get; set;}
	}
}