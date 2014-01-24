using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DailyMetricsForDayQuery
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IAdherenceIdProvider _adherenceIdProvider;

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
																	IAdherenceIdProvider adherenceIdProvider)
		{
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_adherenceIdProvider = adherenceIdProvider;
		}

		public DailyMetricsForDayResult Execute(DateOnlyPeriod period)
		{
			using (var uow = _currentDataSource.Current().Statistic.CreateAndOpenStatelessUnitOfWork())
			{
				var res = uow.Session().CreateSQLQuery(tsql)
					.AddScalar("AnsweredCalls", NHibernateUtil.Int32)
					.AddScalar("AfterCallWorkTime", NHibernateUtil.Int32)
					.AddScalar("TalkTime", NHibernateUtil.Int32)
					.AddScalar("HandlingTime", NHibernateUtil.Int32)
					.AddScalar("ReadyTimePerScheduledReadyTime", NHibernateUtil.Int32)
					.AddScalar("Adherence", NHibernateUtil.Int32)
					.SetDateTime("date_from", period.StartDate)
					.SetDateTime("date_to", period.EndDate)
					.SetInt32("adherence_id", _adherenceIdProvider.Fetch())
					.SetGuid("person_code", _loggedOnUser.CurrentUser().Id.Value)
					.SetGuid("business_unit_code", _currentBusinessUnit.Current().Id.Value) 
					.SetResultTransformer(Transformers.AliasToBean(typeof (DailyMetricsForDayResult)))
					.UniqueResult<DailyMetricsForDayResult>();
				return res ?? new DailyMetricsForDayResult {DataAvailable = false};
			}
		}
	}
}