using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: sumeda herath
    /// Created date: 2008-01-09
    /// </remarks>
    [TestFixture]
    public class PersonSkillTest
    {
        private PersonSkill _target;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            Skill skill = new Skill("test skill", "test", Color.Red, 15, SkillTypeFactory.CreateSkillType());
            Percent percent = new Percent(1);
            _target = new PersonSkill(skill, percent);
        }

        /// <summary>
        /// Verifies the default properties are set.
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
           
             Assert.IsNotNull(_target.Skill);
             Assert.IsNotNull(_target.SkillPercentage);
        }

        /// <summary>
        /// Verifies the person skill can be set and get.
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-10
        /// </remarks>
        [Test]
        public void VerifySkillCanBeSetAndGet()
        {
            ISkill skill = new Skill("test skill", "test", Color.Red, 15, SkillTypeFactory.CreateSkillType());
            _target.Skill = skill;
            ISkill returnSkill = _target.Skill;

            Assert.AreEqual(skill,returnSkill);
        }

        /// <summary>
        /// Verifies the percent can be set and get.
        /// </summary>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-10
        /// </remarks>
        [Test]
        public void VerifyPercentCanBeSetAndGet()
        {
            Percent percent = new Percent(0.9);
            _target.SkillPercentage = percent;
            Percent returnPercent = _target.SkillPercentage;

            Assert.AreEqual(percent, returnPercent);
        }

        
        /// <summary>
        /// Verifies the protected constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-14
        /// </remarks>
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
            PersonSkill clonedEntity = (PersonSkill)_target.Clone();
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        
    }
}
