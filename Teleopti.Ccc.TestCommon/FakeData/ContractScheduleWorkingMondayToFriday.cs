using System;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class ContractScheduleWorkingMondayToFriday : ContractSchedule
	{
		public ContractScheduleWorkingMondayToFriday() : base("WorkingMonToFri")
		{
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.Add(DayOfWeek.Monday, true);
			contractScheduleWeek.Add(DayOfWeek.Tuesday, true);
			contractScheduleWeek.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek.Add(DayOfWeek.Friday, true);
			contractScheduleWeek.Add(DayOfWeek.Saturday, false);
			contractScheduleWeek.Add(DayOfWeek.Sunday, false);
			AddContractScheduleWeek(contractScheduleWeek);
		}
	}
}