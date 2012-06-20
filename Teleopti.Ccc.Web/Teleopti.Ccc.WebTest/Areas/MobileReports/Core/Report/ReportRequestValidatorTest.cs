namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Report
{
	using NUnit.Framework;

	using Rhino.Mocks;

	using SharpTestsEx;

	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
	using Teleopti.Ccc.WebTest.Areas.MobileReports.TestData;
	using Teleopti.Interfaces.Domain;

	[TestFixture]
	public class ReportDataFetcherTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_dataService = MockRepository.GenerateMock<IReportDataService>();
			_target = new ReportDataFetcher(new DefinedReportProviderForTest(), _dataService, new CurrentThreadUserCulture());
		}

		#endregion

		private IReportDataService _dataService;

		private IReportRequestValidator _target;

		[Test]
		public void ShouldAdjustDateToCorrectDatePeriodForWeekInterval()
		{
			var request = new ReportRequestModel
				{
       ReportDate = new DateOnly(2012, 01, 20), ReportIntervalType = 7, ReportId = "GetScheduledAndActual" 
    };
			_dataService.Stub(m => m.GetScheduledAndActual(null)).IgnoreArguments().Return(null);

			var validationResult = _target.FetchData(request);

			validationResult.IsValid().Should().Be.True();
			validationResult.GenerationRequest.ReportInput.Period.StartDate.Should().Be.EqualTo(new DateOnly(2012, 01, 16));
			validationResult.GenerationRequest.ReportInput.Period.EndDate.Should().Be.EqualTo(new DateOnly(2012, 01, 22));
		}

		[Test]
		public void ShouldPopulateErrorsWhenReportIntervalNotValid()
		{
			var request = new ReportRequestModel
				{
       ReportDate = new DateOnly(2012, 01, 20), ReportIntervalType = 0, ReportId = "GetScheduledAndActual" 
    };

			var validationResult = _target.FetchData(request);

			validationResult.IsValid().Should().Be.False();
			validationResult.Errors.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPopulateErrorsWhenReportNotFound()
		{
			var request = new ReportRequestModel
				{
       ReportDate = new DateOnly(2012, 01, 20), ReportIntervalType = 1, ReportId = "NonExistent" 
    };

			var validationResult = _target.FetchData(request);

			validationResult.IsValid().Should().Be.False();
			validationResult.Errors.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldResolveValidDefinedReport()
		{
			var request = new ReportRequestModel
				{
       ReportDate = new DateOnly(2012, 01, 20), ReportIntervalType = 7, ReportId = "GetScheduledAndActual" 
    };
			_dataService.Stub(m => m.GetScheduledAndActual(null)).IgnoreArguments().Return(null);

			var validationResult = _target.FetchData(request);

			validationResult.IsValid().Should().Be.True();
			validationResult.GenerationRequest.Report.ReportId.Should().Be.EqualTo(request.ReportId);
		}
	}
}