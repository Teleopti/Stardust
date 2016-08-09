using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int NumberOfAgents { get; set; }
		public IEnumerable<SiteOpenHour> OpenHours { get; set; }
	}
	public class SiteOpenHour
	{
		public DayOfWeek WeekDay;
		public TimeSpan StartTime;
		public TimeSpan EndTime;
	}
}