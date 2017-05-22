using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		public IList<SkillIntervalStatistics> LoadActualCallPerSkillInterval(IList<ISkill> skills, TimeZoneInfo timeZone, DateOnly today)
		{
			var skillIdArray = skills.Select(x => x.Id.Value.ToString()).ToArray();

			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillIdString = String.Join(",", skillIdArray);

			    var callsPerSkillInterval =
			        uow.Session()
			            .CreateSQLQuery(
			                @"mart.web_intraday_calls_per_skill_interval @time_zone_code=:TimeZone, @today=:Today, @skill_list=:SkillList")
			            .AddScalar("SkillId", NHibernateUtil.Guid)
			            .AddScalar("WorkloadId", NHibernateUtil.Guid)
			            .AddScalar("StartTime", NHibernateUtil.DateTime)
			            .AddScalar("Calls", NHibernateUtil.Double)
			            .AddScalar("AverageHandleTime", NHibernateUtil.Double)
			            .AddScalar("AnsweredCalls", NHibernateUtil.Int32)
			            .AddScalar("HandleTime", NHibernateUtil.Double)
			            .SetString("TimeZone", timeZone.Id)
			            .SetString("Today", today.ToShortDateString(CultureInfo.InvariantCulture))
			            .SetString("SkillList", skillIdString)
			            .SetResultTransformer(Transformers.AliasToBean(typeof(SkillIntervalStatistics)))
			            .List<SkillIntervalStatistics>();

				return callsPerSkillInterval;
			}
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}
