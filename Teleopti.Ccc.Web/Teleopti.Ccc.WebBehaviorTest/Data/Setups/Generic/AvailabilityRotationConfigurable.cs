using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AvailabilityRotationConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public int Days { get; set; }
		public TimeSpan? WorkTimeMinimum { get; set; }
		public TimeSpan? WorkTimeMaximum { get; set; }

		public AvailabilityRotationConfigurable()
		{
			Days = 1;
		}

		public void Apply(IUnitOfWork uow)
		{
			var availabilityRotation = new AvailabilityRotation(Name, Days);
			var availabilityRestriction = new AvailabilityRestriction();

			if (WorkTimeMinimum.HasValue || WorkTimeMaximum.HasValue)
				availabilityRestriction.WorkTimeLimitation = new WorkTimeLimitation(WorkTimeMinimum, WorkTimeMaximum);

			availabilityRotation.AvailabilityDays.ForEach(d => d.Restriction = availabilityRestriction);

			new AvailabilityRepository(uow).Add(availabilityRotation);
		}
	}
}