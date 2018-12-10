using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class PersonDayScheduleSummayViewModel
	{
		public bool IsTerminated { get; set; }
		public bool IsDayOff { get; set; }		
		public DateTimePeriod? DateTimeSpan { get; set; }
		public TimeZoneViewModel Timezone { get; set; }
		public string Title { get; set; }	
		public string Color { get; set; }
		public DateOnly Date { get; set; }
		public int DayOfWeek { get; set; }
		public double ContractTimeMinutes { get; set; }
	}


	public class PersonWeekScheduleViewModel 
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }				
		public IList<PersonDayScheduleSummayViewModel> DaySchedules { get; set; }
		public double ContractTimeMinutes { get; set; }
	}
}