using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonAvailabilityConfigurable : IUserDataSetup
	{
		public DateTime StartDate { get; set; }
		public string Rotation { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var availabilityRotation = AvailabilityRepository.DONT_USE_CTOR(unitOfWork.Current())
				.LoadAll()
				.Single(r => r.Name == Rotation);
			var personAvailability = new PersonAvailability(person, availabilityRotation, new DateOnly(StartDate));
			PersonAvailabilityRepository.DONT_USE_CTOR(unitOfWork.Current()).Add(personAvailability);
		}
	}
}