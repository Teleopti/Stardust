using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class AvailabilityRotationConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public int Days { get; set; }
		public string WorkTimeMinimum { get; set; }
		public string WorkTimeMaximum { get; set; }

		public AvailabilityRotationConfigurable()
		{
			Days = 1;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var availabilityRotation = new AvailabilityRotation(Name, Days);
			var availabilityRestriction = new AvailabilityRestriction();

			if (WorkTimeMinimum != null || WorkTimeMaximum != null)
				availabilityRestriction.WorkTimeLimitation = new WorkTimeLimitation(Transform.ToNullableTimeSpan(WorkTimeMinimum), Transform.ToNullableTimeSpan(WorkTimeMaximum));

			availabilityRotation.AvailabilityDays.ForEach(d => d.Restriction = availabilityRestriction);

			new AvailabilityRepository(currentUnitOfWork.Current()).Add(availabilityRotation);
		}
	}
}