using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class ReportViewModelFactoryTest
	{
		protected class NowMock : INow
		{
			private readonly DateTime _now;

			public NowMock(DateTime now)
			{
				_now = now;
			}

			#region INow Members

			public DateTime Time
			{
				get { return _now; }
			}

			public DateTime UtcTime
			{
				get { return _now.ToUniversalTime(); }
			}

			public DateOnly Date()
			{
				return new DateOnly();
			}

			#endregion
		}

		[Test]
		public void ShouldCreateReportResponseModelByMappingGenerationRequest()
		{
			var reportGenerationRequest = new ReportGenerationResult();
			var reportResponseModel = new ReportResponseModel();

			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			mapper.Stub(x => x.Map<ReportGenerationResult, ReportResponseModel>(reportGenerationRequest)).Return(
				reportResponseModel);

			var target = new ReportViewModelFactory(mapper, null);

			var result = target.GenerateReportDataResponse(reportGenerationRequest);

			result.Should().Be.SameInstanceAs(reportResponseModel);
		}

		[Test]
		public void ShouldCreateViewModelByMappingDate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			DateTime dateTime = DateTime.Today;
			var target = new ReportViewModelFactory(mapper, new NowMock(dateTime));

			var viewModel = new ReportViewModel();

			mapper.Stub(x => x.Map<DateOnly, ReportViewModel>(new DateOnly(dateTime))).Return(viewModel);

			ReportViewModel result = target.CreateReportViewModel();

			result.Should().Be.SameInstanceAs(viewModel);
		}
	}
}