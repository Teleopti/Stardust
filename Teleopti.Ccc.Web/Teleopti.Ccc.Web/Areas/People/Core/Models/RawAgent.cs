using System;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{

	public class OrderAttribute : Attribute
	{
		public OrderAttribute(int order)
		{
			this.Order = order;
		}

		public int Order { get; private set; }
	}
	public class RawAgent : RawUser
	{
		[Order(6)]
		public DateTime StartDate { get; set; }
		[Order(7)]
		public string Organization { get; set; }
		[Order(8)]
		public string Skill { get; set; }
		[Order(9)]
		public string ExternalLogon { get; set; }
		[Order(10)]
		public string Contract { get; set; }
		[Order(11)]
		public string ContractSchedule { get; set; }
		[Order(12)]
		public string PartTimePercentage { get; set; }
		[Order(13)]
		public string ShiftBag { get; set; }
		[Order(14)]
		public string SchedulePeriodType { get; set; }
		[Order(15)]
		public double SchedulePeriodLength { get; set; }
	}
}


