using System;
using System.Globalization;
using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class LatestStatisticsIntervalIdLoader : ILatestStatisticsIntervalIdLoader
	{
		public int? Load(Guid[] skillIdList, DateOnly today)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillListString = String.Join(",", skillIdList.Select(id => id.ToString()).ToArray());

				var intervalId = uow.Session().CreateSQLQuery(@"mart.web_intraday_latest_interval @today=:Today, @skill_list=:SkillList")
					.SetString("Today", today.ToShortDateString(CultureInfo.InvariantCulture))
					.SetParameter("SkillList", skillListString, NHibernateUtil.StringClob)
					.UniqueResult<int>();
				if (intervalId == -1)
					return null;

				return intervalId;
			}
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}