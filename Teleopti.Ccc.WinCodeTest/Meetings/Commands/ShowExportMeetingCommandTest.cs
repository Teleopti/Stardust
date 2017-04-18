using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
     [TestFixture]
    public class ShowExportMeetingCommandTest
    {

        private MockRepository _mocks;
        private ShowExportMeetingCommand _target;
        private IExportMeetingPresenter _exportMeetingPresenter;
        private IExportableScenarioProvider _exportableScenarioProvider;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _exportMeetingPresenter = _mocks.StrictMock<IExportMeetingPresenter>();
            _exportableScenarioProvider = _mocks.StrictMock<IExportableScenarioProvider>();
            _target = new ShowExportMeetingCommand(_exportMeetingPresenter, _exportableScenarioProvider);

        }

        [Test]
        public void CanExecuteShouldBeFalseIfDatesAreIncorrect()
        {
            Expect.Call(_exportableScenarioProvider.AllowedScenarios()).Return(new List<IScenario> { new Scenario("d") });
            Expect.Call(_exportMeetingPresenter.Show);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }
}