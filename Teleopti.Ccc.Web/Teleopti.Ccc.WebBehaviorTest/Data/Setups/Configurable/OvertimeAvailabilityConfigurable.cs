using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class OvertimeAvailabilityConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var endTime = Transform.ToTimeSpan(EndTime);
			var overtimeAvailability = new OvertimeAvailability(user, new DateOnly(Date), Transform.ToTimeSpan(StartTime),
																EndTimeNextDay ? endTime.Add(new TimeSpan(1, 0, 0, 0)) : endTime);

			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(uow);
			overtimeAvailabilityRepository.Add(overtimeAvailability);
		}
	}
}