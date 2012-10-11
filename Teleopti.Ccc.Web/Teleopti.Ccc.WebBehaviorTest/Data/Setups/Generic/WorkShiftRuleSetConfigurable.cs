using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
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
			Segment = "00:15";
			Blacklisted = false;
		}

		public void Apply(IUnitOfWork uow)
		{
			var activity = new ActivityRepository(uow).LoadAll().Single(a => a.Name == Activity);
			var shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(a => a.Description.Name == ShiftCategory);

			var start = new TimePeriodWithSegment(Transform.ToTimePeriod(StartBoundry), Transform.ToTimeSpan(Segment));
			var end = new TimePeriodWithSegment(Transform.ToTimePeriod(EndBoundry), Transform.ToTimeSpan(Segment));

			var generator = new WorkShiftTemplateGenerator(activity, start, end, shiftCategory);

			var ruleSet = new WorkShiftRuleSet(generator)
			              	{
			              		Description = new Description(Name), 
			              		OnlyForRestrictions = Blacklisted
			              	};

			new WorkShiftRuleSetRepository(uow).Add(ruleSet);
		}
	}
}