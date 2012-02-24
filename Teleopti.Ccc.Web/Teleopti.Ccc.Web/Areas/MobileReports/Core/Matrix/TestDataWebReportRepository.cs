using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix
{
	using System.Globalization;

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
			return CreateData(intervalType, intervalTo, (s, d1, d2, d3, p) => new ReportDataForecastVersusActualWorkload(s, d1, d2, p));
		}

		private static IEnumerable<T> CreateData<T>(int intervalType, int intervalTo, Func<string, decimal,decimal,decimal,int, T> makeMe)
		{
			var currentCulture = CultureInfo.CurrentCulture;
			if (intervalType == 7)
			{
				return
					Enumerable.Range(0, 7).Select( /* Proc always seems to return days in 1-7 where 1 always Monday */
						i => makeMe.Invoke(string.Format(currentCulture.DateTimeFormat.DayNames[(i + 1) % 7], i * 15), 1M * i, 2M * i, 0.01M * i, i+1));
			}
			return
				Enumerable.Range(0, intervalTo + 1).Select(
					i => makeMe.Invoke(string.Format("00:{0:00}", i * 15), 1M * i, 2M * i, 0.01M * i, i+1));
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
			return CreateData(intervalType, intervalTo, (s, d1, d2, d3, p) => new ReportDataQueueStatAbandoned(s, d1, d2, p));
			
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
			return CreateData(intervalType, intervalTo, (s, d1, d2, d3, p) => new ReportDataServiceLevelAgentsReady(s, d1, d2, d3, p));
		}

		#endregion
	}
}