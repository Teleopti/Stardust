using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class SkillGroupReducerForCascadingSkillsTest
	{
		private SkillGroupReducerForCascadingSkills _target;

		[SetUp]
		public void Setup()
		{
			_target = new SkillGroupReducerForCascadingSkills();
		}

		[Test]
		public void ShouldOnlyKeepHighestLevelSkillOnPerson()
		{
			var activityA = ActivityFactory.CreateActivity("A");
			var lowLevelSkill = SkillFactory.CreateSkill("Z.Skill");
			lowLevelSkill.Activity = activityA;
			var highLevelSkill = SkillFactory.CreateSkill("B.Skill");
			highLevelSkill.Activity = activityA;
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
				new List<ISkill> {lowLevelSkill, highLevelSkill});

			_target.ReduceToPrimarySkill(person.Period(DateOnly.MinValue));

			var personSkillCollection = person.Period(DateOnly.MinValue).PersonSkillCollection;
			personSkillCollection.Count().Should().Be.EqualTo(2);
			var primarySkill = personSkillCollection.Where(p => p.Skill.Equals(highLevelSkill));
			primarySkill.First().Active.Should().Be.True();
			var secondarySkill = personSkillCollection.Where(p => p.Skill.Equals(lowLevelSkill));
			secondarySkill.First().Active.Should().Be.False();
		}

		[Test]
		public void ShouldHandlePersonWithoutSkill()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
				new List<ISkill>());

			_target.ReduceToPrimarySkill(person.Period(DateOnly.MinValue));

			var personSkillCollection = person.Period(DateOnly.MinValue).PersonSkillCollection;
			personSkillCollection.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void AllActivitiesMustStillBeRepresentedAfterReduction()
		{
			var activityA = ActivityFactory.CreateActivity("A");
			var lowLevelSkillActivityA = SkillFactory.CreateSkill("Z.Skill");
			lowLevelSkillActivityA.Activity = activityA;
			var highLevelSkillActivityA = SkillFactory.CreateSkill("B.Skill");
			highLevelSkillActivityA.Activity = activityA;
			var activityB = ActivityFactory.CreateActivity("B");
			var lowLevelSkillActivityB = SkillFactory.CreateSkill("F.Skill");
			lowLevelSkillActivityB.Activity = activityB;

			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
				new List<ISkill> { lowLevelSkillActivityA, highLevelSkillActivityA, lowLevelSkillActivityB });

			_target.ReduceToPrimarySkill(person.Period(DateOnly.MinValue));

			var personSkillCollection = person.Period(DateOnly.MinValue).PersonSkillCollection;
			personSkillCollection.Count().Should().Be.EqualTo(3);
			var primarySkillA = personSkillCollection.Where(p => p.Skill.Equals(highLevelSkillActivityA));
			primarySkillA.First().Active.Should().Be.True();
			var secondarySkillA = personSkillCollection.Where(p => p.Skill.Equals(lowLevelSkillActivityA));
			secondarySkillA.First().Active.Should().Be.False();
			var primarySkillB = personSkillCollection.Where(p => p.Skill.Equals(lowLevelSkillActivityB));
			primarySkillB.First().Active.Should().Be.True();
		}
	}
}