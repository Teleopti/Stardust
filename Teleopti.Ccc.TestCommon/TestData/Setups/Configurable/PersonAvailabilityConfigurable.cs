using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAvailabilityConfigurable : IUserDataSetup
	{
		public DateTime StartDate { get; set; }
		public string Rotation { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var availabilityRotation = new AvailabilityRepository(uow)
				.LoadAll()
				.Single(r => r.Name == Rotation);
			var personAvailability = new PersonAvailability(user, availabilityRotation, new DateOnly(StartDate));
			new PersonAvailabilityRepository(uow).Add(personAvailability);
		}
	}
}