using System.Globalization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class ReportDataFetcher : IReportRequestValidator
	{
		private readonly IReportDataService _dataService;
		private readonly IDefinedReportProvider _definedReportProvider;

		public ReportDataFetcher(IDefinedReportProvider definedReportProvider, IReportDataService dataService)
		{
			_definedReportProvider = definedReportProvider;
			_dataService = dataService;
		}

		#region IReportRequestValidator Members

		public ReportDataFetchResult FetchData(ReportRequestModel request)
		{
			int interval = request.ReportIntervalType;
			if (!(interval == 1 || interval == 7))
			{
				// TODO PW Check This
				return Error(Resources.InputError);
			}

			var reportDataParam = getReportDataParameters(request, interval);

			var report = _definedReportProvider.Get(request.ReportId);
			if (report == null)
			{
				// TODO PW Check This
				return Error(Resources.InputError);
			}

			var reportData = report.GenerateReport(_dataService, reportDataParam);

			return new ReportDataFetchResult
			       	{
			       		GenerationRequest =
			       			new ReportGenerationResult {ReportInput = reportDataParam, Report = report, ReportData = reportData}
			       	};
		}

		#endregion

		private static ReportDataParam getReportDataParameters(ReportRequestModel request, int interval)
		{
			DateOnly firstDay = interval == 7
			                    	? new DateOnly(DateHelper.GetFirstDateInWeek(request.ReportDate, CultureInfo.CurrentCulture))
			                    	: request.ReportDate;
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(interval - 1));

			return new ReportDataParam {IntervalType = interval, Period = period, SkillSet = request.SkillSet};
		}

		private ReportDataFetchResult Error(string message)
		{
			return new ReportDataFetchResult {Errors = new[] {message}};
		}
	}
}