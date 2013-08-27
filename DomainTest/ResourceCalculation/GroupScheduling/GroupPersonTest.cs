using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
    [TestFixture]
    public class GroupPersonTest
    {
        private IGroupPerson _groupPerson;
        private MockRepository _mock;
        readonly DateOnly _dateOnly = new DateOnly(2000, 1, 1);
        private IPerson _person1;
        private IPerson _person2;
        private IWorkShiftRuleSet _ruleSet1;
        private IWorkShiftRuleSet _ruleSet2;
        private IWorkShiftRuleSet _ruleSet3;
    	private IPersonPeriod _personPeriod;
		private IPersonPeriod _personPeriod2;
    	private IRuleSetBag _bag1;
		private IRuleSetBag _bag2;
    	private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _person1 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly,Enumerable.Empty<ISkill>());
            _person2 = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly,Enumerable.Empty<ISkill>());
			_person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
            _personPeriod = _person1.Period(_dateOnly);
			_personPeriod2 = _person2.Period(_dateOnly);
			_bag1 = new RuleSetBag();
			_bag2 = new RuleSetBag();
        	_guid = Guid.NewGuid();
        }

        private void SetExpectations()
        {
            var skill = SkillFactory.CreateSkill("Phone");
            var skillPercent = new Percent(1);
            var personSkill = new PersonSkill(skill, skillPercent);
            
            var category1 = new ShiftCategory("kat1");
            var category2 = new ShiftCategory("kat2");
            var generator1 = _mock.StrictMock<IWorkShiftTemplateGenerator>();
            var generator2 = _mock.StrictMock<IWorkShiftTemplateGenerator>();
        
            _ruleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            _ruleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
            _ruleSet3 = _mock.StrictMock<IWorkShiftRuleSet>();
           
            _bag1.AddRuleSet(_ruleSet1);
            _bag1.AddRuleSet(_ruleSet2);
           
            _bag2.AddRuleSet(_ruleSet3);

	        _person1.AddSkill(personSkill, _personPeriod);
            personSkill.SkillPercentage = new Percent(2);

            Expect.Call(_ruleSet1.TemplateGenerator).Return(generator1).Repeat.Any();
            Expect.Call(_ruleSet2.TemplateGenerator).Return(generator2).Repeat.Any();
            Expect.Call(_ruleSet3.TemplateGenerator).Return(generator1).Repeat.Any();

            Expect.Call(generator1.Category).Return(category1).Repeat.Any();
            Expect.Call(generator2.Category).Return(category2).Repeat.Any();
        }

		private void SetRuleSetBagExpectations()
		{
			_personPeriod.RuleSetBag = _bag1;
			_personPeriod2.RuleSetBag = _bag2;
		}

		private void SetRuleSetBagExpectationsNoBag()
		{
			_personPeriod.RuleSetBag = _bag1;
			_personPeriod2.RuleSetBag = null;
		}

        [Test]
        public void VerifyPersonPeriodAndSchedulePeriod()
        {
			var persons = new List<IPerson> { _person1, _person2 };
        	
			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
	        using (_mock.Playback())
	        {
		        var factory = new GroupPersonFactory();
		        _groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

		        var person = _groupPerson as Person;
		        Assert.IsNotNull(person);
		        Assert.IsNotNull(person.Period(_dateOnly));
		        Assert.IsNotNull(person.SchedulePeriod(_dateOnly));
		        Assert.IsNotNull(person.VirtualSchedulePeriod(_dateOnly));
		        Assert.IsTrue(person.VirtualSchedulePeriod(_dateOnly).IsValid);
	        }
        }

        [Test]
        public void CanGetListOfPersons()
        {
			var persons = new List<IPerson> { _person1, _person2 };
			
			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
	        using (_mock.Playback())
	        {
		        var factory = new GroupPersonFactory();
		        _groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

		        Assert.AreEqual(2, _groupPerson.GroupMembers.Count());
	        }
        }

        [Test]
        public void CanGetAListOfPersonalSkillsOnDate()
        {
			var persons = new List<IPerson> { _person1, _person2 };
			
			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
	        using (_mock.Playback())
	        {
		        var factory = new GroupPersonFactory();
		        _groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

		        var personalSkills = ((Person) _groupPerson).PersonPeriodCollection[0].PersonSkillCollection;
		        Assert.IsNotNull(personalSkills);
		        Assert.AreEqual(1, personalSkills.Count());
	        }
        }

        [Test]
        public void CombinedPersonSkillsShouldNotAlterOriginalPersonSkill()
        {
            ISkill skill = SkillFactory.CreateSkill("hej");
            IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { skill });
            IPerson person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { skill });
            var persons = new List<IPerson> { person1, person2 };
            Assert.AreEqual(new Percent(1), person1.PersonPeriodCollection[0].PersonSkillCollection.First().SkillPercentage);
            var factory = new GroupPersonFactory();
			_groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);
            var personalSkills = ((Person)_groupPerson).PersonPeriodCollection[0].PersonSkillCollection;
            Assert.IsNotNull(personalSkills);
            Assert.AreEqual(1, personalSkills.Count());
            Assert.AreEqual(new Percent(1), person1.PersonPeriodCollection[0].PersonSkillCollection.First().SkillPercentage);
        }

        [Test]
        public void CommonRuleSetBagRemovesCategoryThatIsNotCommon()
        {
			var persons = new List<IPerson> { _person1, _person2 };
			
			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
	        using (_mock.Playback())
	        {
		        var factory = new GroupPersonFactory();
		        _groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

		        var bag = ((Person) _groupPerson).PersonPeriodCollection[0].RuleSetBag;
		        Assert.AreEqual(2, bag.RuleSetCollection.Count);
		        Assert.IsTrue(bag.RuleSetCollection.Contains(_ruleSet1));
		        Assert.IsTrue(bag.RuleSetCollection.Contains(_ruleSet3));
		        Assert.IsFalse(bag.RuleSetCollection.Contains(_ruleSet2));
	        }
        }

        [Test]
        public void ShouldBeSameRuleSetBagInVirtualSchedulePeriodAsOnFirstPersonPeriod()
        {
			var persons = new List<IPerson> { _person1, _person2 };
			
			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
	        using (_mock.Playback())
	        {
		        var factory = new GroupPersonFactory();
		        _groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

		        var bag = ((Person) _groupPerson).PersonPeriodCollection[0].RuleSetBag;
		        Assert.That(bag.Equals(((Person) _groupPerson).Period(_dateOnly).RuleSetBag));
	        }
        }

		[Test]
		public void ShouldReturnEmptyRuleSetBagsWhenAgentHasNoBag()
		{
			var persons = new List<IPerson> { _person1, _person2 };

			using(_mock.Record())
			{
				SetRuleSetBagExpectationsNoBag();
				SetExpectations();
			}
			using (_mock.Playback())
			{
				var factory = new GroupPersonFactory();
				_groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);

				Assert.AreEqual(0, ((Person) _groupPerson).Period(_dateOnly).RuleSetBag.RuleSetCollection.Count);
			}
		}

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShouldThrowAnExceptionIfListOfPersonsIsEmpty()
        {
            var persons = new List<IPerson> ();

			_groupPerson = new GroupPerson(persons, _dateOnly, "", _guid);
        }

		[Test]
		public void ShouldNotAddGroupMemberWithoutPersonPeriodOnDate()
		{
            var skillPercent = new Percent(1);
		    var skill = SkillFactory.CreateSkill("hej");
            var personSkill = new PersonSkill(skill, skillPercent);
			
			_ruleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
			_ruleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
			_ruleSet3 = _mock.StrictMock<IWorkShiftRuleSet>();

			_bag1.AddRuleSet(_ruleSet1);
			_bag1.AddRuleSet(_ruleSet2);

			_bag2.AddRuleSet(_ruleSet3);
			var persons = new List<IPerson> { _person1, _person2 };

			_person2.DeletePersonPeriod(_personPeriod2);
			_person1.AddSkill(personSkill,_personPeriod);
			_personPeriod.RuleSetBag = _bag1;
			
			using(_mock.Record())
			{
			}
			using (_mock.Playback())
			{
				var factory = new GroupPersonFactory();
				_groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "NAME", _guid);
				Assert.AreEqual(1, _groupPerson.GroupMembers.Count());
			}
		}

		[Test]
		public void ShouldSetIdFromConstructorParameter()
		{
			var persons = new List<IPerson> { _person1, _person2 };
			var factory = new GroupPersonFactory();

			using(_mock.Record())
			{
				SetRuleSetBagExpectations();
				SetExpectations();
			}
			using (_mock.Playback())
			{
				_groupPerson = factory.CreateGroupPerson(persons, _dateOnly, "Name", _guid);

				Assert.AreEqual(_guid, _groupPerson.Id);
			}
		}
    }
}
