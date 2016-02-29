using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntradayMonitorDataLoader : IIntradayMonitorDataLoader
    {
		public MonitorDataViewModel Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
				var skillListString = String.Join(",", skillList.Select(id => id.ToString()).ToArray());
	            return
		            uow.Session()
			            .CreateSQLQuery("exec mart.web_intraday @time_zone_code=:TimeZone, @today=:Today, @skill_list=:SkillList")
						.AddScalar("ForecastedCalls", NHibernateUtil.Double)
			            .AddScalar("OfferedCalls", NHibernateUtil.Double)
						.AddScalar("LatestStatsTime", NHibernateUtil.DateTime)
						.AddScalar("ForecastedActualCallsDiff", NHibernateUtil.Double)
			            .SetReadOnly(true)
			            .SetString("TimeZone", timeZone.Id)
			            .SetString("Today", DateOnly.Today.ToShortDateString(CultureInfo.InvariantCulture))
			            .SetString("SkillList", skillListString)
			            .SetResultTransformer(Transformers.AliasToBean(typeof (MonitorDataViewModel)))
			            .SetReadOnly(true)
			            .UniqueResult<MonitorDataViewModel>();
            }
        }

        private IUnitOfWorkFactory statisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity) TeleoptiPrincipal.CurrentPrincipal.Identity);
            return identity.DataSource.Analytics;
        }
    }
}
