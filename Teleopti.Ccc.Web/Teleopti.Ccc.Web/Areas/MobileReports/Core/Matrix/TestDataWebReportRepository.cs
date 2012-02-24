using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix
{
	public class TestDataWebReportRepository : IWebReportRepository
	{
		#region IWebReportRepository Members

		public IEnumerable<ReportControlSkillGet> ReportControlSkillGet(Guid reportId, Guid personCode, int languageId,
		                                                                   Guid businessUnitCode)
		{
			return new List<ReportControlSkillGet>
			       	{
			       		new ReportControlSkillGet {Id = -2, Name = "All"},
			       		new ReportControlSkillGet {Id = -1, Name = "Not Defined"},
			       		new ReportControlSkillGet {Id = 1, Name = "Test Skill 1"},
			       		new ReportControlSkillGet {Id = 2, Name = "Test Skill 2"},
			       		new ReportControlSkillGet {Id = 4, Name = "Test Skill 4"},
			       		new ReportControlSkillGet {Id = 3, Name = "Test Skill 3"}
			       	};
		}

		public ReportMobileReportInit ReportMobileReportInit(Guid personCode, int languageId, Guid businessUnitCode,
		                                                        string skillSet, string timeZoneCode)
		{
			return new ReportMobileReportInit
			       	{IntervalFrom = 0, IntervalTo = 96, Scenario = 0, TimeZone = 0, SkillSet = "1,2,5,67", WorkloadSet = "1,2,3,4,5,6,7,8,9"};
		}

		public IEnumerable<ReportDataForecastVersusActualWorkload> ReportDataForecastVersusActualWorkload(int scenarioId,
		                                                                                                  string skillSet,
		                                                                                                  string workloadSet,
		                                                                                                  int intervalType,
		                                                                                                  DateTime dateFrom,
		                                                                                                  DateTime dateTo,
		                                                                                                  int intervalFrom,
		                                                                                                  int intervalTo,
		                                                                                                  int timeZoneId,
		                                                                                                  Guid personCode,
		                                                                                                  Guid reportId,
		                                                                                                  int languageId,
		                                                                                                  Guid
		                                                                                                  	businessUnitCode)
		{
			return Enumerable.Range(0, intervalTo + 1).Select(
				i => new ReportDataForecastVersusActualWorkload(string.Format("00:{0:00}", i*15), 1M*i, 2M*i));
		}

		public IEnumerable<ReportDataQueueStatAbandoned> ReportDataQueueStatAbandoned(int scenarioId, string skillSet,
		                                                                              string workloadSet, int intervalType,
		                                                                              DateTime dateFrom, DateTime dateTo,
		                                                                              int intervalFrom, int intervalTo,
		                                                                              int timeZoneId,
		                                                                              Guid personCode, Guid reportId,
		                                                                              int languageId,
		                                                                              Guid businessUnitCode)
		{
			return Enumerable.Range(0, intervalTo + 1).Select(
				i => new ReportDataQueueStatAbandoned(string.Format("00:{0:00}", i*15), 1M*i, 2M*i));
		}

		public IEnumerable<ReportDataServiceLevelAgentsReady> ReportDataServiceLevelAgentsReady(string skillSet,
		                                                                                        string workloadSet,
		                                                                                        int intervalType,
		                                                                                        DateTime dateFrom,
		                                                                                        DateTime dateTo,
		                                                                                        int intervalFrom,
		                                                                                        int intervalTo,
		                                                                                        int serviceLevelCalculationId,
		                                                                                        int timeZoneId,
		                                                                                        Guid personCode, Guid reportId,
		                                                                                        int languageId,
		                                                                                        Guid businessUnitCode)
		{
			return Enumerable.Range(0, intervalTo + 1).Select(
				i => new ReportDataServiceLevelAgentsReady(string.Format("00:{0:00}", i*15), 1M*i, 2M*i, 0.01M*i));
		}

		#endregion
	}
}