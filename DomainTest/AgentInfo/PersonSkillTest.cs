using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
			Assert.AreNotEqual(clonedEntity, _target);
			Assert.AreNotSame(clonedEntity, _target);

			clonedEntity = (IPersonSkill)_target.Clone();
			Assert.AreNotEqual(clonedEntity, _target);
			Assert.AreNotSame(clonedEntity, _target);
        }
    }
}
