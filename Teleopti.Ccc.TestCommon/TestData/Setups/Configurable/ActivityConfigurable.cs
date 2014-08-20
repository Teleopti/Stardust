using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ActivityConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Color { get; set; }
		public bool? AllowMeeting { get; set; }
		public bool? InReadyTime { get; set; }
		public string BusinessUnit { get; set; }
		public bool? InWorkTime { get; set; }

	    public IActivity Activity;

		public void Apply(IUnitOfWork uow)
		{
            Activity = new Activity(Name);

			if (Color != null)
                Activity.DisplayColor = System.Drawing.Color.FromName(Color);

			if (AllowMeeting.HasValue)
			{
                Activity.AllowOverwrite = AllowMeeting.Value;
			}

			if (InReadyTime.HasValue)
			{
				Activity.InReadyTime = InReadyTime.Value;
			}

			if (InWorkTime.HasValue)
			{
				Activity.InWorkTime = InWorkTime.Value;
			}

			if(!string.IsNullOrEmpty(BusinessUnit))
				Activity.SetBusinessUnit(new BusinessUnitRepository(uow).LoadAll().Single(b => b.Name == BusinessUnit));

			var activityRepository = new ActivityRepository(uow);
			activityRepository.Add(Activity);
		}
	}
}