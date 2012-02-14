using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class DefinedReportProvider : IDefinedReportProvider
	{
		private static readonly IList<DefinedReportInformation> ReportInformations = new[]
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
																											LegendResourceKeys = new [] {"AnsweredCalls", "AbandonedCalls"}
																											/*
																											 * Period = r.Period,
			                                                                                            		Y1 = r.CallsAnswered,
			                                                                                            		Y2 = r.CallsAbandoned
																											 * */

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
																												LegendResourceKeys = new [] {"ForecastedCalls", "OfferedCalls"}
																												/*
																							Period = r.Period,
			                                                                   	     		Y1 = r.ForecastedCalls,
			                                                                   	     		Y2 = r.OfferedCalls
			                                                                   	     	* */
		                                                                             			},
		                                                                             		new DefinedReportInformation
		                                                                             			{
		                                                                             				ReportId = "GetScheduledAndActual",
		                                                                             				ReportName = "GetScheduledAndActual",
		                                                                             				ReportNameResourceKey =
		                                                                             					"ResReportServiceLevelAndAgentsReady",
		                                                                             				FunctionCode =
		                                                                             					"ResReportForecastvsActualWorkload",
		                                                                             				GenerateReport =
		                                                                             					(service, input) =>
		                                                                             					service.GetScheduledAndActual(input),
																										LegendResourceKeys = new [] {"ScheduledAgentsReady", "AgentsReady"}
																										
																										/*
																										 * Period = r.Period,
			                                                              	     		Y1 = r.ScheduledAgentsReady,
			                                                              	     		Y2 = r.AgentsReady*/
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
																										LegendResourceKeys = new [] {"ServiceLevelPercentSignColon", null}
																										/*
																										 * Period = r.Period,
			                                                              	     		Y1 = r.ServiceLevel*100M,
			                                                              	     		Y2 = 0M
																										 * */
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