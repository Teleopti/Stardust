using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		public IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillListString = String.Join(",", skillList.Select(id => id.ToString()).ToArray());
				return
					uow.Session()
						.CreateSQLQuery("exec mart.web_intraday @time_zone_code=:TimeZone, @today=:Today, @skill_list=:SkillList")
						.AddScalar("IntervalId", NHibernateUtil.Int16)
						.AddScalar("IntervalDate", NHibernateUtil.DateTime)
						.AddScalar("ForecastedCalls", NHibernateUtil.Double)
						.AddScalar("ForecastedHandleTime", NHibernateUtil.Double)
						.AddScalar("ForecastedAverageHandleTime", NHibernateUtil.Double)
						.AddScalar("CalculatedCalls", NHibernateUtil.Double)
						.AddScalar("HandleTime", NHibernateUtil.Double)
						.AddScalar("AverageHandleTime", NHibernateUtil.Double)
						.AddScalar("AnsweredCalls", NHibernateUtil.Double)
						.AddScalar("AnsweredCallsWithinSL", NHibernateUtil.Double)
						.AddScalar("ServiceLevel", NHibernateUtil.Double)
						.AddScalar("AbandonedCalls", NHibernateUtil.Double)
						.AddScalar("AbandonedRate", NHibernateUtil.Double)
						.AddScalar("SpeedOfAnswer", NHibernateUtil.Double)
						.AddScalar("AverageSpeedOfAnswer", NHibernateUtil.Double)
						.SetString("TimeZone", timeZone.Id)
						.SetString("Today", today.ToShortDateString(CultureInfo.InvariantCulture))
						.SetParameter("SkillList", skillListString, NHibernateUtil.StringClob)
						.SetResultTransformer(Transformers.AliasToBean(typeof(IncomingIntervalModel)))
						.SetReadOnly(true)
						.List<IncomingIntervalModel>();
			}
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}
