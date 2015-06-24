using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class CampaignForm
	{
		public string Name { get; set; }
		public int CallListLen { get; set; }
		public int ConnectRate { get; set; }
		public int RightPartyConnectRate { get; set; }
		public int ConnectAverageHandlingTime { get; set; }
		public int RightPartyAverageHandlingTime { get; set; }
		public int UnproductiveTime { get; set; }
		public ActivityViewModel Activity { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IList<CampaignWorkingHour> WorkingHours { get; set; }
	}

	public class CampaignWorkingHour
	{
		public DayOfWeek WeekDay;
		public TimePeriod WorkingPeriod;
	}

	public class ActivityViewModel
	{
		public Guid? Id;
		public string Name;
		public bool IsSelected;
	}
}