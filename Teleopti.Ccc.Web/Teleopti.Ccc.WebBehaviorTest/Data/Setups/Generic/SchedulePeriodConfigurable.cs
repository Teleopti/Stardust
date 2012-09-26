using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
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