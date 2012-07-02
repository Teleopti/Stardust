namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	using System;

	using UserTexts;
	using Matrix;
	using Providers;
	using Models.Domain;
	using Models.Report;
	using Interfaces.Domain;

	public class ReportDataFetcher : IReportRequestValidator
	{
		private readonly IUserCulture _userCulture;

		private readonly IReportDataService _dataService;

		private readonly IDefinedReportProvider _definedReportProvider;

		public ReportDataFetcher(
			IDefinedReportProvider definedReportProvider, IReportDataService dataService, IUserCulture _userCulture)
		{
			_definedReportProvider = definedReportProvider;
			_dataService = dataService;
			this._userCulture = _userCulture;
		}

		#region IReportRequestValidator Members

		public ReportDataFetchResult FetchData(ReportRequestModel request)
		{
			var interval = (ReportIntervalType)request.ReportIntervalType;
			if (!(interval == ReportIntervalType.Day || interval == ReportIntervalType.Week))
			{
				// TODO PW Check This
				return Error(Resources.InputError);
			}

			var report = _definedReportProvider.Get(request.ReportId);
			if (report == null)
			{
				// TODO PW Check This
				return Error(Resources.InputError);
			}

			var reportDataParam = getReportDataParameters(request, interval, report.ForeignId);

			var reportData = report.GenerateReport(_dataService, reportDataParam);

			return new ReportDataFetchResult
				{
					GenerationRequest =
						new ReportGenerationResult { ReportInput = reportDataParam, Report = report, ReportData = reportData }
				};
		}

		#endregion

		private ReportDataParam getReportDataParameters(ReportRequestModel request, ReportIntervalType interval, Guid foreignId)
		{
			
			DateOnly firstDay = interval.IsTypeWeek()
			                    	? new DateOnly(DateHelper.GetFirstDateInWeek(request.ReportDate, _userCulture.GetCulture()))
			                    	: request.ReportDate;
			var addDays = interval.IsTypeWeek() ? 6 : 0;
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(addDays));

			return new ReportDataParam { IntervalType = interval, Period = period, SkillSet = request.SkillSet, ReportId = foreignId };
		}

		private ReportDataFetchResult Error(string message)
		{
			return new ReportDataFetchResult { Errors = new[] { message } };
		}
	}
}