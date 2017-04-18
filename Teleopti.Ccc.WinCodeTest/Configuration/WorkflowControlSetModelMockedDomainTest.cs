using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class WorkflowControlSetModelMockedDomainTest
    {
        private WorkflowControlSetModel _target;
        private IWorkflowControlSet _domainEntity;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _domainEntity = _mocks.StrictMock<IWorkflowControlSet>();
            _target = new WorkflowControlSetModel(_domainEntity, _domainEntity);
        }

        [Test]
        public void ShouldAddSkillToMatchList()
        {
            var skill = _mocks.DynamicMock<ISkill>();
            _mocks.Record();
            _domainEntity.AddSkillToMatchList(skill);
            _mocks.ReplayAll();
            _target.AddSkillToMatchList(skill);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveSkillFromMatchList()
        {
            var skill = _mocks.DynamicMock<ISkill>();
            _mocks.Record();
            _domainEntity.RemoveSkillFromMatchList(skill);
            _mocks.ReplayAll();
            _target.RemoveSkillFromMatchList(skill);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldHaveSkillMatchList()
        {
            var expectedSkills = new ReadOnlyCollection<ISkill>(new[] { _mocks.DynamicMock<ISkill>() });
            _mocks.Record();
            Expect.Call(_domainEntity.MustMatchSkills).Return(expectedSkills);
            _mocks.ReplayAll();
            var actual = _target.MustMatchSkills;
            _mocks.VerifyAll();
            Assert.That(actual, Is.SameAs(expectedSkills));
        }

        [Test]
        public void ShouldSetAutoGrantOn()
        {
            _mocks.Record();
            Expect.Call(_domainEntity.AutoGrantShiftTradeRequest = true);
            Expect.Call(_domainEntity.AutoGrantShiftTradeRequest).Return(false);
            _mocks.ReplayAll();
            _target.AutoGrantShiftTradeRequest = true;
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetAutoGrantOff()
        {
            _mocks.Record();
            Expect.Call(_domainEntity.AutoGrantShiftTradeRequest = false);
            Expect.Call(_domainEntity.AutoGrantShiftTradeRequest).Return(true);
            _mocks.ReplayAll();
            _target.AutoGrantShiftTradeRequest = false;
            _mocks.VerifyAll();
        }

        [Test]
        public void CanReadAutoGrantProperty()
        {
            _mocks.Record();
            Expect.Call(_domainEntity.AutoGrantShiftTradeRequest).Return(true);
            _mocks.ReplayAll();
            Assert.IsTrue(_domainEntity.AutoGrantShiftTradeRequest);
        }
    }
}