using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class TeamBlockRuleSetBagExtractorTest
    {
        [Test]
        public void ShouldReturnEmptyListIfTeamBlockIsEmpty()
        {
			var target = new RuleSetBagExtractor();
			var teamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(), new List<IList<IScheduleMatrixPro>>()), new BlockInfo(new DateOnlyPeriod()));

			Assert.AreEqual(0, target.GetRuleSetBag(teamBlockInfo).Count());
        }

        [Test]
        public void ShouldReturnRuleSetBagForSigleAgentSingleDay()
		{
			var ruleSetBag1 = new RuleSetBag();
			
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			person1.PersonPeriodCollection[0].RuleSetBag = ruleSetBag1;

			var target = new RuleSetBagExtractor();
			var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
			var persons = new List<IPerson> { person1 };

			var teamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(persons,"test"), new List<IList<IScheduleMatrixPro>>()), new BlockInfo(dateOnlyPeriod));

	        Assert.AreEqual(1, target.GetRuleSetBag(teamBlockInfo).Count());
        }

        [Test]
        public void ShouldReturnRuleSetBagForSigleAgentMultipleDays()
		{
			var ruleSetBag1 = new RuleSetBag();
			var ruleSetBag3 = new RuleSetBag();
			
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			person1.PersonPeriodCollection[0].RuleSetBag = ruleSetBag1;
			
			var newPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 3, 11));
			newPeriod1.RuleSetBag = ruleSetBag3;
			person1.AddPersonPeriod(newPeriod1);

			var target = new RuleSetBagExtractor();
			var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
			var persons = new List<IPerson> {person1};

			var teamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(persons, "test"), new List<IList<IScheduleMatrixPro>>()), new BlockInfo(dateOnlyPeriod));

	        Assert.AreEqual(2, target.GetRuleSetBag(teamBlockInfo).Count());
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamSingleDay()
        {
			var ruleSetBag1 = new RuleSetBag();
			var ruleSetBag2 = new RuleSetBag();

			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			person1.PersonPeriodCollection[0].RuleSetBag = ruleSetBag1;

			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			person2.PersonPeriodCollection[0].RuleSetBag = ruleSetBag2;

			var target = new RuleSetBagExtractor();
			var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
			var persons = new List<IPerson> { person1, person2 };

			var teamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(persons, "test"), new List<IList<IScheduleMatrixPro>>()), new BlockInfo(dateOnlyPeriod));

	        Assert.AreEqual(2, target.GetRuleSetBag(teamBlockInfo).Count());
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamMultipleDays()
        {
			var ruleSetBag1 = new RuleSetBag();
			var ruleSetBag2 = new RuleSetBag();
			var ruleSetBag3 = new RuleSetBag();
			var ruleSetBag4 = new RuleSetBag();

			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
	        person1.PersonPeriodCollection[0].RuleSetBag = ruleSetBag1;

			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			person2.PersonPeriodCollection[0].RuleSetBag = ruleSetBag2;

			var newPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 3, 11));
	        newPeriod1.RuleSetBag = ruleSetBag3;
			person1.AddPersonPeriod(newPeriod1);

			var newPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 3, 11));
			newPeriod2.RuleSetBag = ruleSetBag4;
			person2.AddPersonPeriod(newPeriod2);

			var target = new RuleSetBagExtractor();
			var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
			var persons = new List<IPerson> {person1, person2};

			var teamBlockInfo = new TeamBlockInfo(new TeamInfo(new Group(persons, "test"), new List<IList<IScheduleMatrixPro>>()), new BlockInfo(dateOnlyPeriod));
			Assert.AreEqual(4, target.GetRuleSetBag(teamBlockInfo).Count());
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamSingleDateOnly()
        {
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2014, 3, 10));
			var ruleSetBag1 = new RuleSetBag();
	        person1.PersonPeriodCollection[0].RuleSetBag = ruleSetBag1;
			var target = new RuleSetBagExtractor();

	        Assert.IsNotNull(target.GetRuleSetBagForTeamMember(person1, new DateOnly(2014, 03, 10)));
        }
    }
}
