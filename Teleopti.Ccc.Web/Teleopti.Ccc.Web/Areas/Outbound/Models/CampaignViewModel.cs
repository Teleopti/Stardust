using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class CampaignViewModel
	{
		public Guid? Id;
		public string Name;
		public ActivityViewModel Activity;  
		public int CallListLen;
		public int TargetRate;
		public int ConnectRate;
		public int RightPartyConnectRate;
		public int ConnectAverageHandlingTime;
		public int RightPartyAverageHandlingTime;
		public int UnproductiveTime;
		public DateOnly StartDate;
		public DateOnly EndDate;
		public IEnumerable<CampaignWorkingHour> WorkingHours { get; set; }
	}

	public class CampaignStatistics
	{
		public int Planned;
		public int PlannedWarning;
		public int Scheduled;
		public int ScheduledWarning;
		public int OnGoing;
		public int OnGoingWarning;
		public int Done;
	}
}