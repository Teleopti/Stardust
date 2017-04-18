using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class CanModifyMeetingTest
    {
        private MockRepository _mocks;
        private IMeetingOverviewViewModel _model;
        private CanModifyMeeting _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IMeetingOverviewViewModel>();
            _target = new CanModifyMeeting(_model);
        }

        [Test]
        public void ShouldReturnFalseIfScenarioRestrictedAndNotAllowed()
        {
            var scenario = _mocks.StrictMock<Scenario>();
            Expect.Call(_model.CurrentScenario).Return(scenario);
            Expect.Call(scenario.Restricted).Return(true);
            _mocks.ReplayAll();
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                _target.CanExecute.Should().Be.False();
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfModifyMeetingNotAllowed()
        {
            var scenario = _mocks.StrictMock<Scenario>();
            Expect.Call(_model.CurrentScenario).Return(scenario);
            Expect.Call(scenario.Restricted).Return(false);
            _mocks.ReplayAll();
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                _target.CanExecute.Should().Be.False();
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfModifyMeetingAllowed()
        {
            var scenario = _mocks.StrictMock<Scenario>();
            Expect.Call(_model.CurrentScenario).Return(scenario);
            Expect.Call(scenario.Restricted).Return(false);
            _mocks.ReplayAll();
            
             _target.CanExecute.Should().Be.True();
           
            _mocks.VerifyAll();
        }
    }

}