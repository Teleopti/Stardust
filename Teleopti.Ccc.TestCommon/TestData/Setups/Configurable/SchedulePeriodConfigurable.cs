using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SchedulePeriodConfigurable : IUserSetup
	{
		public DateTime StartDate { get; set; }
		public SchedulePeriodType Type { get; set; }
		public int Length { get; set; }
		public int MustHavePreference { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var schedulePeriod = new Domain.Scheduling.Assignment.SchedulePeriod(
				new DateOnly(StartDate),
				Type,
				Length);
			user.AddSchedulePeriod(schedulePeriod);
			schedulePeriod.MustHavePreference = MustHavePreference;
		}

	}
}