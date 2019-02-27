using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class RuleSetBagWithTwoCategories : IUserDataSetup
	{
		private readonly BaseShiftCategory _cat1;
		private readonly BaseShiftCategory _cat2;
		private readonly TimePeriodWithSegment _start1;
		private readonly TimePeriodWithSegment _end1;
		private readonly TimePeriodWithSegment _start2;
		private readonly TimePeriodWithSegment _end2;

		public Domain.Scheduling.ShiftCreator.RuleSetBag TheRuleSetBag;

		public RuleSetBagWithTwoCategories(BaseShiftCategory cat1, int start1, int end1, BaseShiftCategory cat2, int start2, int end2)
		{
			_cat1 = cat1;
			_cat2 = cat2;
			_start1 = new TimePeriodWithSegment(new TimePeriod(start1, 0, start1, 0), new TimeSpan(0, 15, 0));
			_end1 = new TimePeriodWithSegment(new TimePeriod(end1, 0, end1, 0), new TimeSpan(0, 15, 0));
			_start2 = new TimePeriodWithSegment(new TimePeriod(start2, 0, start2, 0), new TimeSpan(0, 15, 0));
			_end2 = new TimePeriodWithSegment(new TimePeriod(end2, 0, end2, 0), new TimeSpan(0, 15, 0));
		}

		public RuleSetBagWithTwoCategories(BaseShiftCategory cat1, int earliestStart1, int latestStart1, int earliestEnd1, int latestEnd1, SecondShiftCategory cat2, int earliestStart2, int latestStart2, int earliestEnd2, int latestEnd2)
		{
			_cat1 = cat1;
			_cat2 = cat2;
			_start1 = new TimePeriodWithSegment(new TimePeriod(earliestStart1, 0, latestStart1, 0), new TimeSpan(0, 15, 0));
			_end1 = new TimePeriodWithSegment(new TimePeriod(earliestEnd1, 0, latestEnd1, 0), new TimeSpan(0, 15, 0));
			_start2 = new TimePeriodWithSegment(new TimePeriod(earliestStart2, 0, latestStart2, 0), new TimeSpan(0, 15, 0));
			_end2 = new TimePeriodWithSegment(new TimePeriod(earliestEnd2, 0, latestEnd2, 0), new TimeSpan(0, 15, 0));
		}


		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
			var activityRepository = ActivityRepository.DONT_USE_CTOR(unitOfWork, null, null);
			activityRepository.Add(activity);

			TheRuleSetBag = new Domain.Scheduling.ShiftCreator.RuleSetBag();
			var generator1 = new WorkShiftTemplateGenerator(activity, _start1, _end1, _cat1.ShiftCategory);
			var ruleSet1 = new WorkShiftRuleSet(generator1);
			var generator2 = new WorkShiftTemplateGenerator(activity, _start2, _end2, _cat2.ShiftCategory);
			var ruleSet2 = new WorkShiftRuleSet(generator2);

			ruleSet1.Description = new Description("Regeln 1");
			ruleSet2.Description = new Description("Regeln 2");
			TheRuleSetBag.Description = new Description("Påsen");
			TheRuleSetBag.AddRuleSet(ruleSet1);
			TheRuleSetBag.AddRuleSet(ruleSet2);

			var workShiftRuleSetRepository = new WorkShiftRuleSetRepository(unitOfWork);
			workShiftRuleSetRepository.Add(ruleSet1);
			workShiftRuleSetRepository.Add(ruleSet2);
			new RuleSetBagRepository(unitOfWork).Add(TheRuleSetBag);

			unitOfWork.Current().Reassociate(person);
			person.Period(DateOnlyForBehaviorTests.TestToday).RuleSetBag = TheRuleSetBag;
			Debug.Assert(unitOfWork.Current().Contains(person));
		}
	}
}