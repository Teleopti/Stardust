using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ActivityConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public bool? AllowMeeting { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var activity = new Activity(Name);

			if (AllowMeeting.HasValue)
			{
				activity.AllowOverwrite = AllowMeeting.Value;
			}

			var activityRepository = new ActivityRepository(uow);
			activityRepository.Add(activity);
		}
	}
}