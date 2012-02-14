using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class NotSkillGroupSpecificationTest
    {
        private NotSkillGroupSpecification _target;
        private MockRepository _mockRepository;
        private IGroupPage _groupPage;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _groupPage = _mockRepository.StrictMock<IGroupPage>();
            _target = new NotSkillGroupSpecification();
        }

        [Test]
        public void VerifyIsSatisfiedFalse()
        {
            using(_mockRepository.Record())
            {
                Expect.Call(_groupPage.DescriptionKey).Return("Skill");
            }
            using(_mockRepository.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_groupPage));
            }
        }

        [Test]
        public void VerifyIsSatisfiedTrue()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_groupPage.DescriptionKey).Return("NotSkill");
            }
            using (_mockRepository.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_groupPage));
            }
        }
    }
}
