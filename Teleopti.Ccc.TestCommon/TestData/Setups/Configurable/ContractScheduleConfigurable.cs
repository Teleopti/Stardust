using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ContractScheduleConfigurable : IDataSetup
	{
		public string Name { get; set; }

		// note: using weekday numbers here will require culture, which we dont know since this is a IDataSetup.
		// changing this to a IUserDataSetup will not work since the PersonPeriod is created before those.
		public bool MondayWorkDay { get; set; }
		public bool TuesdayWorkDay { get; set; }
		public bool WednesdayWorkDay { get; set; }
		public bool ThursdayWorkDay { get; set; }
		public bool FridayWorkDay { get; set; }
		public bool SaturdayWorkDay { get; set; }
		public bool SundayWorkDay { get; set; }

		public IContractSchedule ContractSchedule;

		public ContractScheduleConfigurable()
		{
			SundayWorkDay = false;
			SaturdayWorkDay = false;
			FridayWorkDay = true;
			ThursdayWorkDay = true;
			WednesdayWorkDay = true;
			TuesdayWorkDay = true;
			MondayWorkDay = true;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			if (Name == null)
			{
				Name = RandomName.Make();
			}

			ContractSchedule = ContractScheduleFactory.CreateContractSchedule(Name);
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