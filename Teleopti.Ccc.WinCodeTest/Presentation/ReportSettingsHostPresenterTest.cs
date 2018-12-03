using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;
using Teleopti.Ccc.WinCodeTest.Common;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsHostPresenterTest
    {
        private MockRepository _mocks;
        private ReportSettingsHostPresenter _target;
        private IReportSettingsHostView _view;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IReportSettingsHostView>();
            _target = new ReportSettingsHostPresenter(_view);
        }

        [Test]
        public void ShouldNotShowAnySettingsIfFunctionCodeIsInvalid()
        {
            _target.ShowSettings(new ReportDetail { FunctionPath = "blablabla/my report" });
        }


      
		[Test]
		public void ShouldShowSettingsForScheduleTimeVersusTargetTimeReport()
		{
			var settingsSettingsScheduleTimeVersusTargetTimeView = _mocks.StrictMock<IReportSettingsScheduleTimeVersusTargetTimeView>();

			Expect.Call(_view.GetSettingsForScheduleTimeVersusTargetTimeReport).Return(settingsSettingsScheduleTimeVersusTargetTimeView);
			Expect.Call(() => _view.AddSettingsForScheduleTimeVersusTargetTimeReport(settingsSettingsScheduleTimeVersusTargetTimeView));
			Expect.Call(settingsSettingsScheduleTimeVersusTargetTimeView.InitializeSettings);
			Expect.Call(() => _view.ReportHeaderCheckRightToLeft());
			Expect.Call(() => _view.SetReportFunctionCode("functionCode"));
			Expect.Call(() => _view.SetHeaderText(UserTexts.Resources.ScheduledTimeVsTarget));
			Expect.Call(() => _view.Unfold());
			_mocks.ReplayAll();
			
			var reportDetail = new ReportDetail
			                            	{
			                            		FunctionPath = DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport,
			                            		DisplayName = UserTexts.Resources.ScheduledTimeVsTarget,
												FunctionCode = "functionCode"
			                            	};

			_target.ShowSettings(reportDetail);
			_mocks.VerifyAll();
			Assert.IsNotNull(_target.SettingsForScheduleTimeVersusTargetTimeReport);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Presentation.IReportSettingsHostView.SetHeaderText(System.String)"), Test]
        public void ShouldShowReportNameButDisableSettingsWhenCallComesFromScheduler()
        {
            Expect.Call(() => _view.SetReportFunctionCode("Path"));
            Expect.Call(() => _view.DisableShowSettings());
            Expect.Call(() => _view.ReportHeaderCheckRightToLeft());
            Expect.Call(() => _view.SetHeaderText("Report Header"));
            _mocks.ReplayAll();

        	_target.HideSettingsAndSetReportHeader(new ReportDetail
        	                                       	{
        	                                       		FunctionPath = "Report/Path",
        	                                       		DisplayName = "Report Header",
        	                                       		FunctionCode = "Path"
        	                                       	});
            _mocks.VerifyAll();
        }

    }
}