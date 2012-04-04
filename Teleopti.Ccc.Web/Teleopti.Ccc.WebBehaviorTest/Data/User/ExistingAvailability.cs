using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingAvailability : IUserDataSetup
	{
		private readonly StartTimeLimitation _startTimeLimitation;

		public ExistingAvailability(StartTimeLimitation startTimeLimitation)
		{
			_startTimeLimitation = startTimeLimitation;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var availabilityRestriction = new AvailabilityRestriction {StartTimeLimitation = _startTimeLimitation};
			var availabilityRotation = new AvailabilityRotation("Availability", 1);

			availabilityRotation.AvailabilityDays[0].Restriction = availabilityRestriction;

			var availabilityRotationRepository = new AvailabilityRepository(uow);
			availabilityRotationRepository.Add(availabilityRotation);

			var personAvailabilityRepository = new PersonAvailabilityRepository(uow);
			var personAvailability = new PersonAvailability(user, availabilityRotation, new DateOnly(2001,01,01));
			personAvailabilityRepository.Add(personAvailability);
		}
	}
}