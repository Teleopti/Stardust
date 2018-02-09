using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ActivitySpec
	{
		public string Name { get; set; }
		public string Color { get; set; }
		public bool? AllowMeeting { get; set; }
		public bool? InReadyTime { get; set; }
		public string BusinessUnit { get; set; }
		public bool? InWorkTime { get; set; }

		public bool? RequiresSkill { get; set; }

		public IActivity Activity;
	}

	public class ActivitySetup : IDataSetup<ActivitySpec>
	{
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IActivityRepository _activities;

		public ActivitySetup(IBusinessUnitRepository businessUnits, IActivityRepository activities)
		{
			_businessUnits = businessUnits;
			_activities = activities;
		}

		public void Apply(ActivitySpec spec)
		{
			spec.Activity = new Activity(spec.Name) {RequiresSkill = true};

			if (spec.Color != null)
				spec.Activity.DisplayColor = System.Drawing.Color.FromName(spec.Color);

			if (spec.AllowMeeting.HasValue)
			{
				spec.Activity.AllowOverwrite = spec.AllowMeeting.Value;
			}

			if (spec.InReadyTime.HasValue)
			{
				spec.Activity.InReadyTime = spec.InReadyTime.Value;
			}

			if (spec.InWorkTime.HasValue)
			{
				spec.Activity.InWorkTime = spec.InWorkTime.Value;
			}

			if (spec.RequiresSkill.HasValue)
				spec.Activity.RequiresSkill = spec.RequiresSkill.Value;

			if (!string.IsNullOrEmpty(spec.BusinessUnit))
				spec.Activity.SetBusinessUnit(_businessUnits.LoadAll().Single(b => b.Name == spec.BusinessUnit));

			_activities.Add(spec.Activity);
		}
	}
}