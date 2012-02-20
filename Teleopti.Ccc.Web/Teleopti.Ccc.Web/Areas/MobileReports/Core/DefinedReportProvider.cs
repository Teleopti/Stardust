using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class DefinedReportProvider : IDefinedReportProvider
	{
		private static readonly IList<DefinedReportInformation>
			ReportInformations = new[]
			                     	{
			                     		new DefinedReportInformation
			                     			{
			                     				ReportId = "GetAnsweredAndAbandoned",
			                     				ReportName =
			                     					"GetAnsweredAndAbandoned",
			                     				ReportNameResourceKey =
			                     					"ResReportAbandonmentAndSpeedOfAnswer",
			                     				FunctionCode =
			                     					"ResReportAbandonmentAndSpeedOfAnswer",
			                     				GenerateReport =
			                     					(service, input) =>
			                     					service.GetAnsweredAndAbandoned(
			                     						input),
			                     				ReportInfo = new ReportMetaInfo
			                     				             	{
			                     				             		SeriesResourceKeys =
			                     				             			new[]
			                     				             				{
			                     				             					"AnsweredCalls",
			                     				             					"AbandonedCalls"
			                     				             				},
			                     				             		SeriesFixedDecimalHint = new[] {0, 0},
			                     				             		ChartTypeHint =
			                     				             			new[]
			                     				             				{"stackedline", "stackedbar"}
			                     				             	}
			                     			},
			                     		new DefinedReportInformation
			                     			{
			                     				ReportId =
			                     					"GetForeCastVsActualWorkload",
			                     				ReportName =
			                     					"GetForeCastVsActualWorkload",
			                     				ReportNameResourceKey =
			                     					"ResReportForecastvsActualWorkload",
			                     				FunctionCode =
			                     					"ResReportForecastvsActualWorkload",
			                     				GenerateReport =
			                     					(service, input) =>
			                     					service.
			                     						GetForecastVersusActualWorkload(
			                     							input),
			                     				ReportInfo = new ReportMetaInfo
			                     				             	{
			                     				             		SeriesResourceKeys =
			                     				             			new[]
			                     				             				{
			                     				             					"ForecastedCalls",
			                     				             					"OfferedCalls"
			                     				             				},
			                     				             		SeriesFixedDecimalHint = new[] {1, 0},
			                     				             		ChartTypeHint =
			                     				             			new[]
			                     				             				{"line", "line"}
			                     				             	}
			                     			},
			                     		new DefinedReportInformation
			                     			{
			                     				ReportId = "GetScheduledAndActual",
			                     				ReportName = "GetScheduledAndActual",
			                     				ReportNameResourceKey =
			                     					"ResReportServiceLevelAndAgentsReady",
			                     				FunctionCode =
			                     					"ResReportServiceLevelAndAgentsReady",
			                     				GenerateReport =
			                     					(service, input) =>
			                     					service.GetScheduledAndActual(input),
			                     				ReportInfo = new ReportMetaInfo
			                     				             	{
			                     				             		SeriesResourceKeys =
			                     				             			new[]
			                     				             				{
			                     				             					"ScheduledAgentsReady"
			                     				             					, "AgentsReady"
			                     				             				},
			                     				             		SeriesFixedDecimalHint = new[] {0, 0},
			                     				             		ChartTypeHint =
			                     				             			new[]
			                     				             				{"line", "line"}
			                     				             	}
			                     			}
			                     		,
			                     		new DefinedReportInformation
			                     			{
			                     				ReportId = "GetServiceLevelAgent",
			                     				ReportName = "GetServiceLevelAgent",
			                     				ReportNameResourceKey =
			                     					"GetSlaNotLoc",
			                     				FunctionCode =
			                     					"ResReportServiceLevelAndAgentsReady",
			                     				GenerateReport =
			                     					(service, input) =>
			                     					service.GetServiceLevelAgent(input),
			                     				ReportInfo = new ReportMetaInfo
			                     				             	{
			                     				             		SeriesResourceKeys =
			                     				             			new[]
			                     				             				{
			                     				             					"ServiceLevelPercentSignColon"
			                     				             					, null
			                     				             				},
			                     				             		SeriesFixedDecimalHint = new[] {0, 0},
			                     				             		ChartTypeHint =
			                     				             			new[]
			                     				             				{"line", "line"}
			                     				             	}
			                     			}
			                     	};

		#region IDefinedReportProvider Members

		public IEnumerable<DefinedReportInformation> GetDefinedReports()
		{
			// TODO: Filter by permission (Wrap to extract ReportId from External Provider?)
			return ReportInformations;
		}

		public IDefinedReport Get(string reportId)
		{
			return GetDefinedReports().FirstOrDefault(r => r.ReportId.Equals(reportId));
		}

		#endregion
	}
}