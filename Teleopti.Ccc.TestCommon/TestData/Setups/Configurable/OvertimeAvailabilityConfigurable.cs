using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class OvertimeAvailabilityConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var endTime = TimeSpan.Parse(EndTime);
			var overtimeAvailability = new OvertimeAvailability(person, new DateOnly(Date), TimeSpan.Parse(StartTime),
																EndTimeNextDay ? endTime.Add(new TimeSpan(1, 0, 0, 0)) : endTime);

			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(unitOfWork);
			overtimeAvailabilityRepository.Add(overtimeAvailability);
		}
	}
}