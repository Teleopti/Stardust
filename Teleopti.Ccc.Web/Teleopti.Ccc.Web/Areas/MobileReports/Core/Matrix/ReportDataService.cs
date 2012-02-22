using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix
{
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public class ReportDataService : IReportDataService
	{
		private readonly Func<IWebReportRepository> _webReportRepository;
		private readonly IWebReportUserInfoProvider _webReportUserInfo;

		public ReportDataService(Func<IWebReportRepository> webReportRepository, IWebReportUserInfoProvider webReportUserInfo)
		{
			_webReportRepository = webReportRepository;
			_webReportUserInfo = webReportUserInfo;
		}

		#region IReportDataService Members

		public IEnumerable<ReportDataPeriodEntry> GetAnsweredAndAbandoned(ReportDataParam param)
		{
			// TODO: Extract this id from IExternalFunctionsProvider? PW
			const int reportId = 10;
			WebReportUserInformation user = _webReportUserInfo.GetUserInformation();
			IWebReportRepository webReportRepository = _webReportRepository.Invoke();
			ReportMobileReportInit init = webReportRepository.ReportMobileReportInit(user.PersonCode, user.LanguageId,
			                                                                             user.BusinessUnitCode, param.SkillSet,
			                                                                             user.TimeZoneCode);
			return webReportRepository.ReportDataQueueStatAbandoned(init.Scenario, init.SkillSet, init.WorkloadSet,
			                                                         param.IntervalType,
			                                                         param.Period.StartDate, param.Period.EndDate,
			                                                         init.IntervalFrom,
			                                                         init.IntervalTo,
			                                                         init.TimeZone, user.PersonCode, reportId, user.LanguageId,
			                                                         user.BusinessUnitCode).Select(r => new ReportDataPeriodEntry
			                                                                                            	{
			                                                                                            		Period = r.Period,
			                                                                                            		Y1 = r.CallsAnswered,
			                                                                                            		Y2 = r.CallsAbandoned,
																												PeriodNumber = r.PeriodNumber
			                                                                                            	});
		}

		public IEnumerable<ReportDataPeriodEntry> GetForecastVersusActualWorkload(ReportDataParam param)
		{
			const int reportId = 10;
			WebReportUserInformation user = _webReportUserInfo.GetUserInformation();
			IWebReportRepository webReportRepository = _webReportRepository.Invoke();
			ReportMobileReportInit init = webReportRepository.ReportMobileReportInit(user.PersonCode, user.LanguageId,
			                                                                             user.BusinessUnitCode, param.SkillSet,
			                                                                             user.TimeZoneCode);
			return webReportRepository.ReportDataForecastVersusActualWorkload(init.Scenario, init.SkillSet, init.WorkloadSet,
			                                                                   param.IntervalType,
			                                                                   param.Period.StartDate, param.Period.EndDate,
			                                                                   init.IntervalFrom,
			                                                                   init.IntervalTo,
			                                                                   init.TimeZone, user.PersonCode, reportId,
			                                                                   user.LanguageId,
			                                                                   user.BusinessUnitCode).Select(
			                                                                   	r => new ReportDataPeriodEntry
			                                                                   	     	{
			                                                                   	     		Period = r.Period,
			                                                                   	     		Y1 = r.ForecastedCalls,
			                                                                   	     		Y2 = r.OfferedCalls,
																							PeriodNumber = r.PeriodNumber
			                                                                   	     	});
		}

		public IEnumerable<ReportDataPeriodEntry> GetScheduledAndActual(ReportDataParam param)
		{
			const int reportId = 10;
			WebReportUserInformation user = _webReportUserInfo.GetUserInformation();
			IWebReportRepository webReportRepository = _webReportRepository.Invoke();
			ReportMobileReportInit init = webReportRepository.ReportMobileReportInit(user.PersonCode, user.LanguageId,
			                                                                             user.BusinessUnitCode, param.SkillSet,
			                                                                             user.TimeZoneCode);
			return webReportRepository.ReportDataServiceLevelAgentsReady(init.SkillSet, init.WorkloadSet,
			                                                              param.IntervalType,
			                                                              param.Period.StartDate, param.Period.EndDate,
			                                                              init.IntervalFrom,
			                                                              init.IntervalTo, init.ServiceLevelCalculationId,
			                                                              init.TimeZone, user.PersonCode, reportId,
			                                                              user.LanguageId,
			                                                              user.BusinessUnitCode).Select(
			                                                              	r => new ReportDataPeriodEntry
			                                                              	     	{
			                                                              	     		Period = r.Period,
			                                                              	     		Y1 = r.ScheduledAgentsReady,
			                                                              	     		Y2 = r.AgentsReady,
																						PeriodNumber = r.PeriodNumber
			                                                              	     	});
		}

		public IEnumerable<ReportDataPeriodEntry> GetServiceLevelAgent(ReportDataParam param)
		{
			const int reportId = 10;
			WebReportUserInformation user = _webReportUserInfo.GetUserInformation();
			IWebReportRepository webReportRepository = _webReportRepository.Invoke();
			ReportMobileReportInit init = webReportRepository.ReportMobileReportInit(user.PersonCode, user.LanguageId,
			                                                                             user.BusinessUnitCode, param.SkillSet,
			                                                                             user.TimeZoneCode);
			return webReportRepository.ReportDataServiceLevelAgentsReady(init.SkillSet, init.WorkloadSet,
			                                                              param.IntervalType,
			                                                              param.Period.StartDate, param.Period.EndDate,
			                                                              init.IntervalFrom,
			                                                              init.IntervalTo, init.ServiceLevelCalculationId,
			                                                              init.TimeZone, user.PersonCode, reportId,
			                                                              user.LanguageId,
			                                                              user.BusinessUnitCode).Select(
			                                                              	r => new ReportDataPeriodEntry
			                                                              	     	{
			                                                              	     		Period = r.Period,
			                                                              	     		Y1 = r.ServiceLevel*100M,
			                                                              	     		Y2 = 0M,
																						PeriodNumber = r.PeriodNumber
			                                                              	     	});
		}

		#endregion
	}
}