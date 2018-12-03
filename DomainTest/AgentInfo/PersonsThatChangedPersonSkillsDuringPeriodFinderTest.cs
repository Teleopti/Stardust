using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture]
	public class PersonsThatChangedPersonSkillsDuringPeriodFinderTest
	{
		private PersonsThatChangedPersonSkillsDuringPeriodFinder _target;

		[SetUp]
		public void Setup()
		{
			_target = new PersonsThatChangedPersonSkillsDuringPeriodFinder();
		}

		[Test]
		public void ShouldFindPersonWithPersonSkillChanged()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			var alteredPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill1});
			alteredPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2016, 3, 9), skill2));
			var staticPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill1});
			var result = _target.Find(new DateOnlyPeriod(2016, 3, 8, 2016, 3, 10),
				new List<IPerson> {alteredPerson, staticPerson});

			result.Count.Should().Be.EqualTo(1);
			var changed = result.First();
			changed.Date.Should().Be.EqualTo(new DateOnly(2016, 3, 9));
			changed.AddedSkills.Count.Should().Be.EqualTo(1);
			changed.AddedSkills.First().Should().Be.EqualTo(skill2);
			changed.RemovedSkills.Count.Should().Be.EqualTo(1);
			changed.RemovedSkills.First().Should().Be.EqualTo(skill1);
		}

		[Test]
		public void ShouldFindPersonWithPersonSkillAdded()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			var alteredPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skill1 });
			alteredPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2016, 3, 9), skill2, skill1));
			var staticPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skill1 });
			var result = _target.Find(new DateOnlyPeriod(2016, 3, 8, 2016, 3, 10),
				new List<IPerson> { alteredPerson, staticPerson });

			result.Count.Should().Be.EqualTo(1);
			var changed = result.First();
			changed.Date.Should().Be.EqualTo(new DateOnly(2016, 3, 9));
			changed.AddedSkills.Count.Should().Be.EqualTo(1);
			changed.AddedSkills.First().Should().Be.EqualTo(skill2);
			changed.RemovedSkills.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFindPersonWithSkillRemoved()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			var alteredPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skill1, skill2 });
			alteredPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2016, 3, 9), skill2));
			var staticPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skill1 });
			var result = _target.Find(new DateOnlyPeriod(2016, 3, 8, 2016, 3, 10),
				new List<IPerson> { alteredPerson, staticPerson });

			result.Count.Should().Be.EqualTo(1);
			var changed = result.First();
			changed.Date.Should().Be.EqualTo(new DateOnly(2016, 3, 9));
			changed.RemovedSkills.Count.Should().Be.EqualTo(1);
			changed.RemovedSkills.First().Should().Be.EqualTo(skill1);
			changed.AddedSkills.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFindPersonWithSkillRemovedAndAddedAgain()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			var alteredPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { skill1, skill2 });
			alteredPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2016, 3, 9), skill2));
			alteredPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2016, 3, 10), skill2, skill1));

			var result = _target.Find(new DateOnlyPeriod(2016, 3, 8, 2016, 3, 10),
				new List<IPerson> { alteredPerson });

			result.Count.Should().Be.EqualTo(2);
			var changed = result.First();
			changed.Date.Should().Be.EqualTo(new DateOnly(2016, 3, 9));
			changed.RemovedSkills.Count.Should().Be.EqualTo(1);
			changed.RemovedSkills.First().Should().Be.EqualTo(skill1);
			changed.AddedSkills.Count.Should().Be.EqualTo(0);

			changed = result.Last();
			changed.Date.Should().Be.EqualTo(new DateOnly(2016, 3, 10));
			changed.AddedSkills.Count.Should().Be.EqualTo(1);
			changed.AddedSkills.First().Should().Be.EqualTo(skill1);
			changed.RemovedSkills.Count.Should().Be.EqualTo(0);
		}
	}
}