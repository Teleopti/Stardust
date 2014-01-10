using AutoMapper;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class ReportViewModelFactory : IReportViewModelFactory
	{
		private readonly IMappingEngine _mappingEngine;
		private readonly INow _now;

		public ReportViewModelFactory(IMappingEngine mappingEngine, INow now)
		{
			_mappingEngine = mappingEngine;
			_now = now;
		}

		#region IReportViewModelFactory Members

		public ReportViewModel CreateReportViewModel()
		{
			var dateOnly = new DateOnly(_now.LocalDateTime());
			return _mappingEngine.Map<DateOnly, ReportViewModel>(dateOnly);
		}

		public ReportResponseModel GenerateReportDataResponse(ReportGenerationResult reportGenerationRequest)
		{
			return _mappingEngine.Map<ReportGenerationResult, ReportResponseModel>(reportGenerationRequest);
		}

		#endregion
	}
}