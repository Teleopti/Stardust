using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ContractScheduleFromTable : IDataSetup
	{
		public IContractSchedule ContractSchedule { get; set; }

		// note: using weekday numbers here will require culture, which we dont know since this is a IDataSetup.
		// changing this to a IUserDataSetup will not work since the PersonPeriod is created before those.
		public bool MondayWorkDay = true;
		public bool TuesdayWorkDay = true;
		public bool WednesdayWorkDay = true;
		public bool ThursdayWorkDay = true;
		public bool FridayWorkDay = true;
		public bool SaturdayWorkDay = false;
		public bool SundayWorkDay = false;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			ContractSchedule = ContractScheduleFactory.CreateContractSchedule("Contract schedule from table");
			var week = new ContractScheduleWeek();
			week.Add(DayOfWeek.Monday, MondayWorkDay);
			week.Add(DayOfWeek.Tuesday, TuesdayWorkDay);
			week.Add(DayOfWeek.Wednesday, WednesdayWorkDay);
			week.Add(DayOfWeek.Thursday, ThursdayWorkDay);
			week.Add(DayOfWeek.Friday, FridayWorkDay);
			week.Add(DayOfWeek.Saturday, SaturdayWorkDay);
			week.Add(DayOfWeek.Sunday, SundayWorkDay);
			ContractSchedule.AddContractScheduleWeek(week);
			ContractScheduleRepository.DONT_USE_CTOR(currentUnitOfWork).Add(ContractSchedule);
		}

	}
}