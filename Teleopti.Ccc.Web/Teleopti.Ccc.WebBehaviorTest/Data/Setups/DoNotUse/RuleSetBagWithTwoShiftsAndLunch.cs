using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class RuleSetBagWithTwoShiftsAndLunch : IUserDataSetup
	{
		private readonly int _start1;
		private readonly int _end1;
		private readonly string _lunchActivity1;
		private readonly int _lunchStart1;
		private readonly int _lunchEnd1;
		private readonly int _start2;
		private readonly int _end2;
		private readonly string _lunchActivity2;
		private readonly int _lunchStart2;
		private readonly int _lunchEnd2;

		public Domain.Scheduling.ShiftCreator.RuleSetBag TheRuleSetBag;

		public RuleSetBagWithTwoShiftsAndLunch(int start1, int end1, string lunchActivity1, int lunchStart1, int lunchEnd1, int start2, int end2, string lunchActivity2, int lunchStart2, int lunchEnd2)
		{
			_start1 = start1;
			_end1 = end1;
			_lunchActivity1 = lunchActivity1;
			_lunchStart1 = lunchStart1;
			_lunchEnd1 = lunchEnd1;
			_start2 = start2;
			_end2 = end2;
			_lunchActivity2 = lunchActivity2;
			_lunchStart2 = lunchStart2;
			_lunchEnd2 = lunchEnd2;
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var start1 = new TimePeriodWithSegment(new TimePeriod(_start1, 0, _start1, 0), new TimeSpan(0, 15, 0));
			var end1 = new TimePeriodWithSegment(new TimePeriod(_end1, 0, _end1, 0), new TimeSpan(0, 15, 0));
			var lunchStart1 = new TimePeriodWithSegment(new TimePeriod(_lunchStart1, 0, _lunchStart1, 0), new TimeSpan(0, 15, 0));
			var lunchLength1 =
				new TimePeriodWithSegment(new TimePeriod(_lunchEnd1 - _lunchStart1, 0, _lunchEnd1 - _lunchStart1, 0),
				                          new TimeSpan(0, 15, 0));
			
			var start2 = new TimePeriodWithSegment(new TimePeriod(_start2, 0, _start2, 0), new TimeSpan(0, 15, 0));
			var end2 = new TimePeriodWithSegment(new TimePeriod(_end2, 0, _end2, 0), new TimeSpan(0, 15, 0));
			var lunchStart2 = new TimePeriodWithSegment(new TimePeriod(_lunchStart2, 0, _lunchStart2, 0), new TimeSpan(0, 15, 0));
			var lunchLength2 =
				new TimePeriodWithSegment(new TimePeriod(_lunchEnd2 - _lunchStart2, 0, _lunchEnd2 - _lunchStart2, 0),
				                          new TimeSpan(0, 15, 0));

			var activityRepository = new ActivityRepository(unitOfWork, null, null);
			var activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
			var activityLunch1 = new ActivityRepository(unitOfWork, null, null).LoadAll().Single(sCat => sCat.Description.Name.Equals(_lunchActivity1));
			var activityLunch2 = new ActivityRepository(unitOfWork, null, null).LoadAll().Single(sCat => sCat.Description.Name.Equals(_lunchActivity2));
			activityRepository.Add(activity);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory(RandomName.Make(), "Purple");
			new ShiftCategoryRepository(unitOfWork).Add(shiftCategory);

			TheRuleSetBag = new Domain.Scheduling.ShiftCreator.RuleSetBag();
			var generator1 = new WorkShiftTemplateGenerator(activity, start1, end1, shiftCategory);
			var ruleSet1 = new WorkShiftRuleSet(generator1);
			var lunch1 = new ActivityAbsoluteStartExtender(activityLunch1, lunchLength1, lunchStart1);
			ruleSet1.AddExtender(lunch1);
			var generator2 = new WorkShiftTemplateGenerator(activity, start2, end2, shiftCategory);
			var ruleSet2 = new WorkShiftRuleSet(generator2);
			var lunch2 = new ActivityAbsoluteStartExtender(activityLunch2, lunchLength2, lunchStart2);
			ruleSet2.AddExtender(lunch2);

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