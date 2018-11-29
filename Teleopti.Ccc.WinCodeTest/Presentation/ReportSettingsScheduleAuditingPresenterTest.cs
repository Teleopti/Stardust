using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduleAuditingPresenterTest
    {
        private ReportSettingsScheduleAuditingPresenter _target;
        private IReportSettingsScheduleAuditingView _view;
        private MockRepository _mocks;
        private IPerson _person;
        private IList<IPerson> _persons;
        private DateOnlyPeriod _changeperiod;
        private DateOnlyPeriod _scheduleperiod;
        private DateOnlyPeriod _changeperiodDisplay;
        private DateOnlyPeriod _scheduleperiodDisplay;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IReportSettingsScheduleAuditingView>();
            _target = new ReportSettingsScheduleAuditingPresenter(_view);
            _person = _mocks.StrictMock<IPerson>();
            _persons = new List<IPerson>{_person};
            _changeperiod = new DateOnlyPeriod(new DateOnly(2010, 10, 31), new DateOnly(2010, 11, 30));
            _scheduleperiod = new DateOnlyPeriod(new DateOnly(2010, 11, 1), new DateOnly(2010, 12, 1));
            _changeperiodDisplay = new DateOnlyPeriod(new DateOnly(2010, 10, 31), new DateOnly(2010, 11, 29));
            _scheduleperiodDisplay = new DateOnlyPeriod(new DateOnly(2010, 11, 1), new DateOnly(2010, 11, 30));
        }

        [Test]
        public void ShouldInitializeReportSettings()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _view.InitUserSelector());
                Expect.Call(() => _view.InitPersonSelector());
                Expect.Call(() => _view.SetDateControlTexts());
            }

            using(_mocks.Playback())
            {
                _target.InitializeSettings();
            }
        }

        [Test]
        public void ShouldReturnSettingsModel()
        {
            using(_mocks.Record())
            {
                Expect.Call(_view.ModifiedBy).Return(_persons);
                Expect.Call(_view.ChangePeriod).Return(_changeperiod);
                Expect.Call(_view.SchedulePeriod).Return(_scheduleperiod);
                Expect.Call(_view.ChangePeriodDisplay).Return(_changeperiodDisplay);
                Expect.Call(_view.SchedulePeriodDisplay).Return(_scheduleperiodDisplay);
                Expect.Call(_view.Agents).Return(_persons);
            }

            using(_mocks.Playback())
            {
                var model = _target.GetSettingsModel;
   
                Assert.AreEqual(_persons, model.ModifiedBy);
                Assert.AreEqual(_changeperiod, model.ChangePeriod);
                Assert.AreEqual(_scheduleperiod, model.SchedulePeriod);
                Assert.AreEqual(_changeperiodDisplay, model.ChangePeriodDisplay);
                Assert.AreEqual(_scheduleperiodDisplay, model.SchedulePeriodDisplay);
                Assert.AreEqual(_persons, model.Agents);
            }
        }
    }
}
