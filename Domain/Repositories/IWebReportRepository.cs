using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IWebReportRepository
	{
		IEnumerable<ReportControlSkillGet> ReportControlSkillGet(Guid reportId, Guid personCode, int languageId,
		                                                            Guid businessUnitCode);

		ReportMobileReportInit ReportMobileReportInit(Guid personCode, int languageId, Guid businessUnitCode, string skillSet, string timeZoneCode);

		IEnumerable<ReportDataForecastVersusActualWorkload> ReportDataForecastVersusActualWorkload(int scenarioId, string skillSet, string workloadSet, int intervalType, DateTime dateFrom, DateTime dateTo, int intervalFrom, int intervalTo, int timeZoneId, Guid personCode, Guid reportId, int languageId, Guid businessUnitCode);

		IEnumerable<ReportDataQueueStatAbandoned> ReportDataQueueStatAbandoned(int scenarioId,
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
		                                                                             Guid businessUnitCode);

		
		IEnumerable<ReportDataServiceLevelAgentsReady> ReportDataServiceLevelAgentsReady(string skillSet,
		                                                                                                 string workloadSet,
		                                                                                                 int intervalType,
		                                                                                                 DateTime dateFrom,
		                                                                                                 DateTime dateTo,
		                                                                                                 int intervalFrom,
		                                                                                                 int intervalTo,
		                                                                                                 int serviceLevelCalculationId,
		                                                                                                 int timeZoneId,
		                                                                                                 Guid personCode,
		                                                                                                 Guid reportId,
		                                                                                                 int languageId,
		                                                                                                 Guid businessUnitCode);
	}
}