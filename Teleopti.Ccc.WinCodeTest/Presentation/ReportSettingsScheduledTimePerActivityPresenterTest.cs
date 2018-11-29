using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduledTimePerActivityPresenterTest
    {
        private MockRepository _mocks;
        private ReportSettingsScheduledTimePerActivityPresenter _target;
        private IReportSettingsScheduledTimePerActivityView _view;
        private readonly IScenario _scenario = new Scenario("scenario");
        private readonly DateOnlyPeriod _period = new DateOnlyPeriod();
        private readonly IList<IPerson> _persons = new List<IPerson>();
        private readonly TimeZoneInfo _timeZone = TimeZoneInfo.Local;
        private readonly IList<IActivity> _activities = new List<IActivity>();

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IReportSettingsScheduledTimePerActivityView>();
            _target = new ReportSettingsScheduledTimePerActivityPresenter(_view);  
        }

        [Test]
        public void VerifyInitializeScheduleTimePerActivity()
        {
            using (_mocks.Record())
            {
                _view.InitAgentSelector();
                _view.InitActivitiesSelector();
                _view.HideTimeZoneControl();
            }

            using (_mocks.Playback())
            {
                _target.InitializeSettings();   
            }   
        }

        [Test]
        public void VerifyGetSettingsModel()
        {
            using (_mocks.Record())
            {
                Expect.Call(_view.Activities).Return(_activities).Repeat.Once();
                Expect.Call(_view.Period).Return(_period).Repeat.Once();
                Expect.Call(_view.TimeZone).Return(_timeZone).Repeat.Once();
                Expect.Call(_view.Persons).Return(_persons).Repeat.Once();
                Expect.Call(_view.Scenario).Return(_scenario).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                _target.GetSettingsModel();
            }
        }
    }
}
