namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Report
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using NUnit.Framework;

	using Rhino.Mocks;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Repositories;
	using Teleopti.Ccc.Domain.WebReport;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

	[TestFixture]
	public class ReportDataServiceTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();

			_webReportRepository = _mock.DynamicMock<IWebReportRepository>();
			_webReportUserInfo = _mock.DynamicMock<IWebReportUserInfoProvider>();

			_target = new ReportDataService(() => _webReportRepository, _webReportUserInfo);
		}

		#endregion

		private void expectUserPreparation()
		{
			Expect.Call(_webReportUserInfo.GetUserInformation()).Return(new WebReportUserInformation());
			Expect.Call(
				_webReportRepository.ReportMobileReportInit(Guid.NewGuid(), 0, Guid.NewGuid(), string.Empty, string.Empty)).
				IgnoreArguments().Return(new ReportMobileReportInit());
		}

		private IReportDataService _target;

		private MockRepository _mock;

		private IWebReportRepository _webReportRepository;

		private IWebReportUserInfoProvider _webReportUserInfo;

		[Test]
		public void VerifyGetAnsweredAndAbandoned()
		{
			var reportDataParam = new ReportDataParam();

			using (_mock.Record())
			{
				expectUserPreparation();
				Expect.Call(_webReportRepository.ReportDataQueueStatAbandoned(0, string.Empty, string.Empty, 0, DateTime.Now,
																			  DateTime.Now, 0, 0, 0, Guid.NewGuid(), Guid.Empty, 0,
				                                                              Guid.NewGuid()))
					.IgnoreArguments().Return(
						new List<ReportDataQueueStatAbandoned>(new List<ReportDataQueueStatAbandoned>
						                                       	{
						                                       		new ReportDataQueueStatAbandoned("00:00", 100M, 10M, 1)
						                                       	}));
			}

			using (_mock.Playback())
			{
				var reportDataPeriodEntries = _target.GetAnsweredAndAbandoned(reportDataParam);

				var reportDataPeriodEntry = reportDataPeriodEntries.First();
				reportDataPeriodEntry.Period.Should().Be.EqualTo("00:00");
				reportDataPeriodEntry.Y1.Should().Be.EqualTo(100M);
				reportDataPeriodEntry.Y2.Should().Be.EqualTo(10M);
				reportDataPeriodEntry.PeriodNumber.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void VerifyReportDataGetForecastVersusActualWorkload()
		{
			var reportDataParam = new ReportDataParam();

			using (_mock.Record())
			{
				expectUserPreparation();

				Expect.Call(_webReportRepository.ReportDataForecastVersusActualWorkload(0, string.Empty, string.Empty, 0,
				                                                                        DateTime.Now,
																						DateTime.Now, 0, 0, 0, Guid.NewGuid(), Guid.Empty, 0,
				                                                                        Guid.NewGuid()))
					.IgnoreArguments().Return(
						new List<ReportDataForecastVersusActualWorkload>(new List<ReportDataForecastVersusActualWorkload>
						                                                 	{
						                                                 		new ReportDataForecastVersusActualWorkload("00:00", 100M,
						                                                 		                                           10M, 2)
						                                                 	}));
			}

			using (_mock.Playback())
			{
				var reportDataPeriodEntries = _target.GetForecastVersusActualWorkload(reportDataParam);

				var reportDataPeriodEntry = reportDataPeriodEntries.First();
				reportDataPeriodEntry.Period.Should().Be.EqualTo("00:00");
				reportDataPeriodEntry.Y1.Should().Be.EqualTo(100M);
				reportDataPeriodEntry.Y2.Should().Be.EqualTo(10M);
				reportDataPeriodEntry.PeriodNumber.Should().Be.EqualTo(2);
			}
		}

		[Test]
		public void VerifyReportDataGetScheduledAndActual()
		{
			var reportDataParam = new ReportDataParam();

			using (_mock.Record())
			{
				expectUserPreparation();

				Expect.Call(_webReportRepository.ReportDataServiceLevelAgentsReady(string.Empty, string.Empty, 0,
				                                                                   DateTime.Now,
																				   DateTime.Now, 0, 0, 0, 0, Guid.NewGuid(), new Guid("AE758403-C16B-40B0-B6B2-E8F6043B6E04"), 0,
				                                                                   Guid.NewGuid()))
					.IgnoreArguments().Return(
						new List<ReportDataServiceLevelAgentsReady>(new List<ReportDataServiceLevelAgentsReady>
						                                            	{
						                                            		new ReportDataServiceLevelAgentsReady("00:00", 110M,
						                                            		                                      11M, 0.12M, 2)
						                                            	}));
			}

			using (_mock.Playback())
			{
				var reportDataPeriodEntries = _target.GetScheduledAndActual(reportDataParam);

				var reportDataPeriodEntry = reportDataPeriodEntries.First();
				reportDataPeriodEntry.Period.Should().Be.EqualTo("00:00");
				reportDataPeriodEntry.Y1.Should().Be.EqualTo(110M);
				reportDataPeriodEntry.Y2.Should().Be.EqualTo(11M);
				reportDataPeriodEntry.PeriodNumber.Should().Be.EqualTo(2);
			}
		}

		[Test]
		public void VerifyReportDataServiceLevelAgentsReady()
		{
			var reportDataParam = new ReportDataParam();

			using (_mock.Record())
			{
				expectUserPreparation();

				Expect.Call(_webReportRepository.ReportDataServiceLevelAgentsReady(string.Empty, string.Empty, 0,
				                                                                   DateTime.Now,
				                                                                   DateTime.Now, 0, 0, 0, 0, Guid.NewGuid(), new Guid("AE758403-C16B-40B0-B6B2-E8F6043B6E04"), 0,
				                                                                   Guid.NewGuid()))
					.IgnoreArguments().Return(
						new List<ReportDataServiceLevelAgentsReady>(new List<ReportDataServiceLevelAgentsReady>
						                                            	{
						                                            		new ReportDataServiceLevelAgentsReady("00:00", 100M,
						                                            		                                      10M, 0.12M, 3)
						                                            	}));
			}

			using (_mock.Playback())
			{
				var reportDataPeriodEntries = _target.GetServiceLevelAgent(reportDataParam);

				var reportDataPeriodEntry = reportDataPeriodEntries.First();
				reportDataPeriodEntry.Period.Should().Be.EqualTo("00:00");
				reportDataPeriodEntry.Y1.Should().Be.EqualTo(12M);
				reportDataPeriodEntry.Y2.Should().Be.EqualTo(0M);
				reportDataPeriodEntry.PeriodNumber.Should().Be.EqualTo(3);
			}
		}
	}
}