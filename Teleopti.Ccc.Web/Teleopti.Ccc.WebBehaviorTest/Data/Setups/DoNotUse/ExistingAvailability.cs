using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingAvailability : IUserDataSetup
	{
		private readonly WorkTimeLimitation _workTimeLimitation;
		private readonly EndTimeLimitation _endTimeLimitation;
		private readonly StartTimeLimitation _startTimeLimitation;

		public ExistingAvailability(StartTimeLimitation startTimeLimitation)
		{
			_startTimeLimitation = startTimeLimitation;
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingAvailability(EndTimeLimitation endTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = endTimeLimitation;
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingAvailability(WorkTimeLimitation workTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = workTimeLimitation;
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var availabilityRestriction = new AvailabilityRestriction
			                              	{
			                              		StartTimeLimitation = _startTimeLimitation,
			                              		EndTimeLimitation = _endTimeLimitation,
			                              		WorkTimeLimitation = _workTimeLimitation
			                              	};
			var availabilityRotation = new AvailabilityRotation("Availability", 1);

			availabilityRotation.AvailabilityDays[0].Restriction = availabilityRestriction;

			var availabilityRotationRepository = AvailabilityRepository.DONT_USE_CTOR(unitOfWork.Current());
			availabilityRotationRepository.Add(availabilityRotation);

			var personAvailabilityRepository = PersonAvailabilityRepository.DONT_USE_CTOR(unitOfWork.Current());
			var personAvailability = new PersonAvailability(person, availabilityRotation, new DateOnly(2001,01,01));
			personAvailabilityRepository.Add(personAvailability);
		}
	}

}