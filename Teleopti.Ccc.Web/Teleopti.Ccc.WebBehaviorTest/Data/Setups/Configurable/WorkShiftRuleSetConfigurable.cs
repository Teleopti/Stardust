using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class WorkShiftRuleSetConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public string Activity { get; set; }
		public string ShiftCategory { get; set; }

		public string StartBoundry { get; set; }
		public string EndBoundry { get; set; }

		public string Segment { get; set; }

		public bool Blacklisted { get; set; }

		public WorkShiftRuleSetConfigurable()
		{
			StartBoundry = "8:00";
			EndBoundry = "17:00";
			Segment = "00:15";
			Blacklisted = false;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var activity = new ActivityRepository(currentUnitOfWork, null, null).LoadAll().Single(a => a.Name == Activity);
			var shiftCategory = new ShiftCategoryRepository(currentUnitOfWork).LoadAll().Single(a => a.Description.Name == ShiftCategory);

			var start = new TimePeriodWithSegment(Transform.ToTimePeriod(StartBoundry), Transform.ToTimeSpan(Segment));
			var end = new TimePeriodWithSegment(Transform.ToTimePeriod(EndBoundry), Transform.ToTimeSpan(Segment));

			var generator = new WorkShiftTemplateGenerator(activity, start, end, shiftCategory);

			var ruleSet = new WorkShiftRuleSet(generator)
			              	{
			              		Description = new Description(Name), 
			              		OnlyForRestrictions = Blacklisted
			              	};

			new WorkShiftRuleSetRepository(currentUnitOfWork).Add(ruleSet);
		}
	}
}