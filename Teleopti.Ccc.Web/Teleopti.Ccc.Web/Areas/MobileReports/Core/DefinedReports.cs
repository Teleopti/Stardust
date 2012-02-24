namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	using System;
	using System.Collections.Generic;

	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public static class DefinedReports
	{
		public static string FunctionCodeResReportAbandonmentAndSpeedOfAnswer = "ResReportAbandonmentAndSpeedOfAnswer"; 
		public static Guid ReportForeignIdResReportAbandonmentAndSpeedOfAnswer = new Guid("C232D751-AEC5-4FD7-A274-7C56B99E8DEC"); 

		public static string FunctionCodeResReportForecastvsActualWorkload = "ResReportForecastvsActualWorkload";
		public static Guid ReportForeignIdResReportForecastvsActualWorkload = new Guid("8D8544E4-6B24-4C1C-8083-CBE7522DD0E0"); 

		public static string FunctionCodeResReportServiceLevelAndAgentsReady = "ResReportServiceLevelAndAgentsReady";
		public static Guid ReportForeignIdResReportServiceLevelAndAgentsReady = new Guid("AE758403-C16B-40B0-B6B2-E8F6043B6E04"); 
		

		public static readonly IList<DefinedReportInformation> ReportInformations = new[]
			{
				new DefinedReportInformation
					{
						ReportId = "GetAnsweredAndAbandoned",
						ForeignId = ReportForeignIdResReportAbandonmentAndSpeedOfAnswer,
						ReportNameResourceKey = "ResReportAbandonmentAndSpeedOfAnswer",
						FunctionCode = FunctionCodeResReportAbandonmentAndSpeedOfAnswer,
						GenerateReport = (service, input) => service.GetAnsweredAndAbandoned(input),
						ReportInfo =
							new ReportMetaInfo
								{
									SeriesResourceKeys = new[] { "AnsweredCalls", "AbandonedCalls" },
									SeriesFixedDecimalHint = new[] { 0, 0 },
									ChartTypeHint = new[] { "stackedline", "stackedbar" }
								}
					},
				new DefinedReportInformation
					{
						ReportId = "GetForeCastVsActualWorkload",
						ForeignId = ReportForeignIdResReportForecastvsActualWorkload,
						ReportNameResourceKey = "ResReportForecastvsActualWorkload",
						FunctionCode = FunctionCodeResReportForecastvsActualWorkload,
						GenerateReport = (service, input) => service.GetForecastVersusActualWorkload(input),
						ReportInfo =
							new ReportMetaInfo
								{
									SeriesResourceKeys = new[] { "ForecastedCalls", "OfferedCalls" },
									SeriesFixedDecimalHint = new[] { 1, 0 },
									ChartTypeHint = new[] { "line", "line" }
								}
					},
				new DefinedReportInformation
					{
						ReportId = "GetScheduledAndActual",
						ForeignId = ReportForeignIdResReportServiceLevelAndAgentsReady,
						ReportNameResourceKey = "ResReportServiceLevelAndAgentsReady",
						FunctionCode = FunctionCodeResReportServiceLevelAndAgentsReady,
						GenerateReport = (service, input) => service.GetScheduledAndActual(input),
						ReportInfo =
							new ReportMetaInfo
								{
									SeriesResourceKeys = new[] { "ScheduledAgentsReady", "AgentsReady" },
									SeriesFixedDecimalHint = new[] { 0, 0 },
									ChartTypeHint = new[] { "line", "line" }
								}
					},
				new DefinedReportInformation
					{
						ReportId = "GetServiceLevelAgent",
						ForeignId = ReportForeignIdResReportServiceLevelAndAgentsReady,
						ReportNameResourceKey = "ServiceLevelParenthesisPercentSign",
						FunctionCode = FunctionCodeResReportServiceLevelAndAgentsReady,
						GenerateReport = (service, input) => service.GetServiceLevelAgent(input),
						ReportInfo =
							new ReportMetaInfo
								{
									SeriesResourceKeys = new[] { "ServiceLevelParenthesisPercentSign", null },
									SeriesFixedDecimalHint = new[] { 0, 0 },
									ChartTypeHint = new[] { "line", "line" }
								}
					}
			};
	}
}