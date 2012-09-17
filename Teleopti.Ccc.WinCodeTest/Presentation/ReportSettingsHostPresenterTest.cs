﻿using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Ccc.WinCode.Reporting;
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
        public void ShouldShowSettingsForScheduledTimePerActivityReport()
        {
            IReportSettingsScheduledTimePerActivityView settingsScheduledTimePerActivityView = _mocks.StrictMock<IReportSettingsScheduledTimePerActivityView>();

            Expect.Call(_view.GetSettingsForScheduledTimePerActivityReport()).Return(settingsScheduledTimePerActivityView);
            Expect.Call(() => _view.AddSettingsForScheduledTimePerActivityReport(settingsScheduledTimePerActivityView));
            Expect.Call(settingsScheduledTimePerActivityView.InitializeSettings);
            Expect.Call(() => _view.ReportHeaderCheckRightToLeft());
            Expect.Call(() => _view.SetHeaderText(UserTexts.Resources.ScheduledTimePerActivity));
			Expect.Call(() => _view.SetReportFunctionCode("functionCode"));
            Expect.Call(() => _view.Unfold());
            _mocks.ReplayAll();

            ReportDetail reportDetail = new ReportDetail
                                            {
                                                FunctionPath = DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport,
                                                DisplayName = UserTexts.Resources.ScheduledTimePerActivity,
												FunctionCode = "functionCode"
                                            };
            _target.ShowSettings(reportDetail);
            _mocks.VerifyAll();

            Assert.IsNotNull(_target.SettingsForScheduledTimePerActivityReport);
        }

        [Test]
        public void ShouldShowSettingsForScheduleAuditingReport()
        {
            IReportSettingsScheduleAuditingView settingsScheduleAuditingView = _mocks.StrictMock<IReportSettingsScheduleAuditingView>();

            Expect.Call(_view.GetSettingsForScheduleAuditingReport).Return(settingsScheduleAuditingView);
            Expect.Call(() => _view.AddSettingsForScheduleAuditingReport(settingsScheduleAuditingView));
            Expect.Call(settingsScheduleAuditingView.InitializeSettings);
            Expect.Call(() => _view.ReportHeaderCheckRightToLeft());
			Expect.Call(() => _view.SetReportFunctionCode("functionCode"));
            Expect.Call(() => _view.SetHeaderText(UserTexts.Resources.ScheduleAuditTrailReport));
            Expect.Call(() => _view.Unfold());
            _mocks.ReplayAll();

            ReportDetail reportDetail = new ReportDetail
            {
                FunctionPath = DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport,
                DisplayName = UserTexts.Resources.ScheduleAuditTrailReport,
				FunctionCode = "functionCode"
            };
            _target.ShowSettings(reportDetail);
            _mocks.VerifyAll();

            Assert.IsNotNull(_target.SettingsForScheduleAuditingReport);
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

        [Test]
        public void ShouldReturnModelForScheduledTimePerActivityReport()
        {
            IReportSettingsScheduledTimePerActivityView reportSettingsScheduledTimePerActivityView =
                _mocks.StrictMock<IReportSettingsScheduledTimePerActivityView>();


            ReportSettingsHostPresenterForTest target = new ReportSettingsHostPresenterForTest(_view,
                                                                                               reportSettingsScheduledTimePerActivityView);

            using (_mocks.Record())
            {
                Expect.Call(reportSettingsScheduledTimePerActivityView.ScheduleTimePerActivitySettingsModel).Return(new ReportSettingsScheduledTimePerActivityModel());
            }

            using (_mocks.Playback())
            {
                ReportSettingsScheduledTimePerActivityModel model = target.GetModelForScheduledTimePerActivityReport();
                Assert.IsNotNull(model);
            }
        }

        [Test]
        public void ShouldReturnModelForScheduleAuditingReport()
        {
            var reportSettingsScheduleAuditingView = _mocks.StrictMock<IReportSettingsScheduleAuditingView>();
            var target = new ReportSettingsHostPresenterForTest(_view, reportSettingsScheduleAuditingView);

            using (_mocks.Record())
            {
                Expect.Call(reportSettingsScheduleAuditingView.ScheduleAuditingSettingsModel).Return(new ReportSettingsScheduleAuditingModel());
            }

            using (_mocks.Playback())
            {
                var model = target.GetModelForScheduleAuditingReport;
                Assert.IsNotNull(model);
            }
        }

        [Test]
        public void ShouldReturnNullIfNoModelForScheduledTimePerActivityReportIsAvailable()
        {
            ReportSettingsScheduledTimePerActivityModel model = _target.GetModelForScheduledTimePerActivityReport();
            Assert.IsNull(model);
        }

        [Test]
        public void ShouldReturnNullIfNoModelForScheduleAuditingReportIsAvailable()
        {
            var model = _target.GetModelForScheduleAuditingReport;
            Assert.IsNull(model);
        }
    }
}