using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class TeamBlockIncludedWorkShiftRuleFilterTest
    {
        private TeamBlockIncludedWorkShiftRuleFilter _target;
		private IWorkShiftTemplateGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _target = new TeamBlockIncludedWorkShiftRuleFilter();
			_generator = new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
											   new TimePeriodWithSegment(10, 0, 12, 0, 60),
											   new TimePeriodWithSegment(11, 0, 13, 0, 60),
											   ShiftCategoryFactory.CreateShiftCategory("sample"));
        }

        [Test]
        public void ShouldReturnEmptyListIfRuleSetBagIsEmpty()
        {
            var ruleSetBags = new List<IRuleSetBag>();
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            Assert.AreEqual(0, _target.Filter(blockPeriod, ruleSetBags).Count());

        }

        [Test]
        public void ShouldReturnRuleSetIfAllDatesAreValid()
        {		
	        var workShiftRuleSet = new WorkShiftRuleSet(_generator);
	        var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			var ruleSetBags = new List<IRuleSetBag> { ruleSetBag };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);

	        var result = _target.Filter(blockPeriod, ruleSetBags);
			Assert.Contains(workShiftRuleSet, result.ToList());
        }

		[Test]
		public void ShouldNotReturnRuleThatIsNotValidInAllDatesInPeriod()
		{
			var workShiftRuleSet = new WorkShiftRuleSet(_generator);
			workShiftRuleSet.AddAccessibilityDate(new DateOnly(2014, 03, 11));
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			var ruleSetBags = new List<IRuleSetBag> { ruleSetBag };
			var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);

			var result = _target.Filter(blockPeriod, ruleSetBags);
			Assert.IsTrue(!result.ToList().Contains(workShiftRuleSet));
		}
    }


}
