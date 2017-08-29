using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAvailabilityConfigurable : IUserDataSetup
	{
		public DateTime StartDate { get; set; }
		public string Rotation { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var availabilityRotation = new AvailabilityRepository(currentUnitOfWork.Current())
				.LoadAll()
				.Single(r => r.Name == Rotation);
			var personAvailability = new PersonAvailability(user, availabilityRotation, new DateOnly(StartDate));
			new PersonAvailabilityRepository(currentUnitOfWork.Current()).Add(personAvailability);
		}
	}
}