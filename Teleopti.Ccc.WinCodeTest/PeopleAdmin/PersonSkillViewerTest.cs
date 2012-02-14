using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    /// <summary>
    /// Tests for PersonSkillViewer
    /// </summary>
    [TestFixture]
    public class PersonSkillViewerTest
    {
        private PersonSkillViewer _target;
        
        [SetUp]
        public void Setup()
        {
            Skill skill = SkillFactory.CreateSkill("Skill For Test", new SkillType("1", 2, "Test"), 1);
            _target = new PersonSkillViewer(skill);
        }

        [Test]
        public void VerifyPropertiesNotNull()
        {
            Assert.IsNotNull(_target.Skill);
            Assert.IsNull(_target.SkillIdentifier);
            Assert.IsNotNull(_target.SkillName);
        }

        [Test]
        public void VerifyTriStateCanSetAndGet()
        {
            Assert.AreEqual(0, _target.TriState);
            _target.TriState++;
            Assert.AreEqual(1, _target.TriState);
        }

        [Test]
        public void VerifySkillCanSet()
        {
            _target.Skill = SkillFactory.CreateSkill("Skill For Test", new SkillType("1", 2, "Test"), 1);
            Assert.AreEqual("Skill For Test", _target.Skill.Name);
        }
    }
}
