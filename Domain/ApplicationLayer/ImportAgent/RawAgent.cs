using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	[AttributeUsage(AttributeTargets.Property)]
	public class OrderAttribute : Attribute
	{
		public OrderAttribute(int order)
		{
			this.Order = order;
		}

		public int Order { get; private set; }
	}

	public class RawAgent
	{
		[Order(0)]
		public string Firstname { get; set; }
		[Order(1)]
		public string Lastname { get; set; }
		[Order(2), Description("Windows user")]
		public string WindowsUser { get; set; }
		[Order(3), Description("Application user")]
		public string ApplicationUserId { get; set; }
		[Order(4)]
		public string Password { get; set; }
		[Order(5)]
		public string Role { get; set; }

		[Order(6), Description("Start date")]
		public DateTime? StartDate { get; set; }

		[Order(7), Description("Site/Team")]
		public string Organization { get; set; }
		[Order(8)]
		public string Skill { get; set; }
		[Order(9), Description("External logon")]
		public string ExternalLogon { get; set; }
		[Order(10)]
		public string Contract { get; set; }
		[Order(11), Description("Contract schedule")]
		public string ContractSchedule { get; set; }
		[Order(12), Description("Part-time percentage")]
		public string PartTimePercentage { get; set; }
		[Order(13), Description("Shift bag")]
		public string ShiftBag { get; set; }
		[Order(14), Description("Schedule period type")]
		public string SchedulePeriodType { get; set; }
		[Order(15), Description("Schedule period length")]
		public double? SchedulePeriodLength { get; set; }

	}
}


