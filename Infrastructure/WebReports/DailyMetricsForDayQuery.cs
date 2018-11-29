using System;
using System.Collections;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DailyMetricsForDayQuery : IDailyMetricsForDayQuery
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		private const string tsql =
									@"exec mart.report_data_agent_schedule_web_result 
									@date_from=:date_from, 
									@date_to=:date_to,
									@person_code=:person_code, 
									@adherence_id=:adherence_id, 
									@business_unit_code=:business_unit_code";

		public DailyMetricsForDayQuery(ILoggedOnUser loggedOnUser,
																	ICurrentDataSource currentDataSource, 
																	ICurrentBusinessUnit currentBusinessUnit,
																	IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public DailyMetricsForDayResult Execute(DateOnly date)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(tsql)
					.AddScalar("AnsweredCalls", NHibernateUtil.Int32)
					.AddScalar("AfterCallWorkTime", NHibernateUtil.Double)
					.AddScalar("TalkTime", NHibernateUtil.Double)
					.AddScalar("HandlingTime", NHibernateUtil.Double)
					.AddScalar("ReadyTimePerScheduledReadyTime", NHibernateUtil.Double)
					.AddScalar("Adherence", NHibernateUtil.Double)
					.SetDateTime("date_from", date.Date)
					.SetDateTime("date_to", date.Date)
					.SetInt32("adherence_id", (int)_globalSettingDataRepository.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()).AdherenceIdForReport())
					.SetGuid("person_code", _loggedOnUser.CurrentUser().Id.Value)
					.SetGuid("business_unit_code", _currentBusinessUnit.Current().Id.Value) 
					.SetResultTransformer(new dailyMetricsForDayResultTransformer())
					.UniqueResult<DailyMetricsForDayResult>();
			}
		}

		private class dailyMetricsForDayResultTransformer : IResultTransformer
		{
			public object TransformTuple(object[] tuple, string[] aliases)
			{
				var ret = new DailyMetricsForDayResult();
				for (var i = 0; i < aliases.Length; i++)
				{
					var tupleValue = tuple[i];
					switch (aliases[i])
					{
						case "AnsweredCalls":
							ret.AnsweredCalls = (int)tupleValue;
							break;
						case "AfterCallWorkTime":
							ret.AfterCallWorkTimeAverage = TimeSpan.FromSeconds((double)tupleValue);
							break;
						case "TalkTime":
							ret.TalkTimeAverage = TimeSpan.FromSeconds((double)tupleValue);
							break;
						case "HandlingTime":
							ret.HandlingTimeAverage = TimeSpan.FromSeconds((double)tupleValue);
							break;
						case "ReadyTimePerScheduledReadyTime":
							ret.ReadyTimePerScheduledReadyTime = new Percent((double)tupleValue);
							break;
						case "Adherence":
							if (tupleValue != null)
							{
								ret.Adherence = new Percent((double)tupleValue);								
							}
							break;
					}
				}
				return ret;
			}

			public IList TransformList(IList collection)
			{
				return collection.Cast<DailyMetricsForDayResult>().ToList();
			}
		}
	}
}