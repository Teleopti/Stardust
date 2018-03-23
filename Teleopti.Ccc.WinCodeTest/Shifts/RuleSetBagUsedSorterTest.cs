using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
	[TestFixture]
	public class RuleSetBagUsedSorterTest
	{
		private RuleSetBagUsedSorter _target;

		[SetUp]
		public void SetUp()
		{
			_target = new RuleSetBagUsedSorter();
		}

		[Test]
		public void ShouldSortRuleSets()
		{
			var templateGenerator = new WorkShiftTemplateGenerator(new Activity(), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory());
			var ruleSetInBag1 = new WorkShiftRuleSet(templateGenerator).WithId();
			var ruleSetInBag2 = new WorkShiftRuleSet(templateGenerator).WithId();
			var ruleSetNotUsed1 = new WorkShiftRuleSet(templateGenerator).WithId();
			var ruleSetNotUsed2 = new WorkShiftRuleSet(templateGenerator).WithId();
			var ruleSets = new List<IWorkShiftRuleSet> { ruleSetNotUsed2, ruleSetInBag2, ruleSetInBag1, ruleSetNotUsed1 };
			ruleSetInBag1.Description = new Description("A_Used");
			ruleSetInBag2.Description = new Description("B_Used");
			ruleSetNotUsed1.Description = new Description("A_NotUsed");
			ruleSetNotUsed2.Description = new Description("B_NotUsed");
			var ruleSetBag = new RuleSetBag(ruleSetInBag2, ruleSetInBag1);

			var result = _target.SortRuleSetsUsedFirst(ruleSets, ruleSetBag);

			result[0].Should().Be.EqualTo(ruleSetInBag1);
			result[1].Should().Be.EqualTo(ruleSetInBag2);
			result[2].Should().Be.EqualTo(ruleSetNotUsed1);
			result[3].Should().Be.EqualTo(ruleSetNotUsed2);
		}

		[Test]
		public void ShouldSortRuleSetBags()
		{
			var templateGenerator = new WorkShiftTemplateGenerator(new Activity(), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory());
			var ruleSet = new WorkShiftRuleSet(templateGenerator).WithId();
			var ruleSetBagWithRuleSet1 = new RuleSetBag(ruleSet){Description = new Description("A_InBag")}.WithId();
			var ruleSetBagWithRuleSet2 = new RuleSetBag(ruleSet) { Description = new Description("B_InBag")}.WithId();
			var ruleSetBagWithoutRuleSet1 = new RuleSetBag{ Description = new Description("A_NotInBag")}.WithId();
			var ruleSetBagWithoutRuleSet2 = new RuleSetBag { Description = new Description("B_NotInBag")}.WithId();
			var ruleSetBags = new List<IRuleSetBag>{ruleSetBagWithoutRuleSet2, ruleSetBagWithRuleSet2, ruleSetBagWithRuleSet1, ruleSetBagWithoutRuleSet1};

			var result = _target.SortRuleSetBagsUsedFirst(ruleSetBags, ruleSet);

			result[0].Should().Be.EqualTo(ruleSetBagWithRuleSet1);
			result[1].Should().Be.EqualTo(ruleSetBagWithRuleSet2);
			result[2].Should().Be.EqualTo(ruleSetBagWithoutRuleSet1);
			result[3].Should().Be.EqualTo(ruleSetBagWithoutRuleSet2);
		}
	}
}
