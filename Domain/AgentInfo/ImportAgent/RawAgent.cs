using System;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{

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
		[Order(2)]
		public string WindowsUser { get; set; }
		[Order(3)]
		public string ApplicationUserId { get; set; }
		[Order(4)]
		public string Password { get; set; }
		[Order(5)]
		public string Role { get; set; }
		
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


