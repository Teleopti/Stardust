using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class CampaignForm
	{
		public Guid? Id { get; set; }
		public string Name { get; set; }
		public int CallListLen { get; set; }
		public int TargetRate { get; set; }
		public int ConnectRate { get; set; }
		public int RightPartyConnectRate { get; set; }
		public int ConnectAverageHandlingTime { get; set; }
		public int RightPartyAverageHandlingTime { get; set; }
		public int UnproductiveTime { get; set; }
		//public DateOnly DurationStart { get; set; }
		//public DateOnly DurationEnd { get; set; }
	}
}