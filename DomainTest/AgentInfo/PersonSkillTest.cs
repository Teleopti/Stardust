using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture]
	public class PersonSkillTest
	{
		private PersonSkill _target;

		[SetUp]
		public void Setup()
		{
			Skill skill = new Skill("test skill", "test", Color.Red, 15, SkillTypeFactory.CreateSkillType());
			Percent percent = new Percent(1);
			_target = new PersonSkill(skill, percent);
		}

		[Test]
		public void VerifyDefaultPropertiesAreSet()
		{
			Assert.IsNotNull(_target.Skill);
		}

		[Test]
		public void VerifyPercentCanBeSetAndGet()
		{
			Percent percent = new Percent(0.9);
			_target.SkillPercentage = percent;
			Percent returnPercent = _target.SkillPercentage;

			Assert.AreEqual(percent, returnPercent);
		}

		[Test]
		public void VerifyProtectedConstructor()
		{
			MockRepository mocks = new MockRepository();
			_target = mocks.StrictMock<PersonSkill>();

			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifyCloneWorks()
		{
			_target.SetId(Guid.NewGuid());

			var clonedEntity = _target.EntityClone();
			Assert.AreEqual(clonedEntity.Id, _target.Id);
			Assert.AreEqual(clonedEntity, _target);
			Assert.AreNotSame(clonedEntity, _target);

			clonedEntity = _target.NoneEntityClone();
			Assert.IsNull(clonedEntity.Id);
			Assert.AreEqual(clonedEntity, _target);
			Assert.AreNotSame(clonedEntity, _target);

			clonedEntity = (IPersonSkill)_target.Clone();
			Assert.AreEqual(clonedEntity, _target);
			Assert.AreNotSame(clonedEntity, _target);
		}

		[Test]
		public void ShouldNotBeEqualWhenOtherIsNull()
		{
			_target.Equals(null).Should().Be.False();
		}

		[Test]
		public void ShouldBeEqualIfSameSkill()
		{
			var personSkill2 = new PersonSkill(_target.Skill, new Percent(1));

			_target.Equals(personSkill2).Should().Be.True();
		}

		[Test]
		public void ShouldBeEqualIfEqualSkill()
		{
			_target.Skill.WithId();
			var personSkill2 = new PersonSkill(_target.Skill.EntityClone(), new Percent(1));

			_target.Equals(personSkill2).Should().Be.True();
		}

		[Test]
		public void ShouldNotBeEqualIfNotSameSkill()
		{
			var skill2 = new Skill("test skill", "test", Color.Red, 15, SkillTypeFactory.CreateSkillType());
			var personSkill2 = new PersonSkill(skill2, new Percent(1));

			_target.Equals(personSkill2).Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEqualIfNotPersonSkill()
		{
			_target.Equals(AbsenceFactory.CreateAbsence("abs").WithId()).Should().Be.False();
		}
	}
}
