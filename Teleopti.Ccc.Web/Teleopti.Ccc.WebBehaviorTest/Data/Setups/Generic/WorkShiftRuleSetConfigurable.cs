using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class WorkShiftRuleSetConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public string Activity { get; set; }
		public string ShiftCategory { get; set; }

		public TimeSpan EarliestStart { get; set; }
		public TimeSpan LatestStart { get; set; }
		public TimeSpan EarliestEnd { get; set; }
		public TimeSpan LatestEnd { get; set; }

		public TimeSpan Segment { get; set; }

		public bool Blacklisted { get; set; }

		public WorkShiftRuleSetConfigurable()
		{
			Segment = TimeSpan.FromMinutes(15);
			Blacklisted = false;
		}

		public void Apply(IUnitOfWork uow)
		{
			var activity = new ActivityRepository(uow).LoadAll().Single(a => a.Name == Activity);
			var shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(a => a.Description.Name == ShiftCategory);

			var start = new TimePeriodWithSegment(new TimePeriod(EarliestStart, LatestStart), Segment);
			var end = new TimePeriodWithSegment(new TimePeriod(EarliestEnd, LatestEnd), Segment);

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