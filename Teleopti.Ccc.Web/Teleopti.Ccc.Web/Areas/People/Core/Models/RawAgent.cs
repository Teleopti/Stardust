using System;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class RawAgent : RawUser
	{
		public DateTime? StartDate { get; set; }
		public string Organization { get; set; }
		public string Skill { get; set; }
		public string ExternalLogon { get; set; }
		public string Contract { get; set; }
		public string ContractSchedule { get; set; }
		public string PartTimePercentage { get; set; }
		public string ShiftBag { get; set; }
		public string SchedulePeriodType { get; set; }
		public double? SchedulePeriodLength { get; set; }
	}
}


