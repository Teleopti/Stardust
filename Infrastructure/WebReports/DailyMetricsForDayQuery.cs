using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	//temp - flytta!
	public class DailyMetricsForDayResult
	{
		public int AnsweredCalls { get; set; }
	}


	public class DailyMetricsForDayQuery
	{
		public DailyMetricsForDayResult Execute(DateOnlyPeriod period, int adherenceType, int timezoneType, IPerson agent, IBusinessUnit businessUnit)
		{
			//temp! injecta nåt för statistic db istället
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string tsql =
					"exec mart.report_data_agent_schedule_web_result @date_from=:date_from, @date_to=:date_to, @time_zone_id=:time_zone_id, @person_code=:person_code, @adherence_id=:adherence_id, @business_unit_code=:business_unit_code";

				return session(uow).CreateSQLQuery(tsql)
				                        .AddScalar("AnsweredCalls", NHibernateUtil.Int32)
				                        .SetDateTime("date_from", period.StartDate)
				                        .SetDateTime("date_to", period.EndDate)
				                        .SetInt32("adherence_id", adherenceType)
				                        .SetInt32("time_zone_id", timezoneType)
				                        .SetGuid("person_code", agent.Id.Value)
				                        .SetGuid("business_unit_code", businessUnit.Id.Value)
										.SetResultTransformer(Transformers.AliasToBean(typeof(DailyMetricsForDayResult)))
										.UniqueResult<DailyMetricsForDayResult>();

			}
		}

		private static IStatelessSession session(IStatelessUnitOfWork uow)
		{
			return ((NHibernateStatelessUnitOfWork)uow).Session;
		}

		private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
			return identity.DataSource.Statistic;
		}
	}
}