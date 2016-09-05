using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		public int? LoadActualWorkloadInSeconds(IList<Guid> skillIdList, TimeZoneInfo timeZone, DateOnly today)
		{

			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillListString = String.Join(",", skillIdList.Select(id => id.ToString()).ToArray());

				var actualWorkloadInSeconds =
					uow.Session()
						.CreateSQLQuery(
							@"mart.web_intraday_workload_in_seconds @time_zone_code=:TimeZone, @today=:Today, @skill_list=:SkillList")
						.SetString("TimeZone", timeZone.Id)
						.SetString("Today", today.ToShortDateString(CultureInfo.InvariantCulture))
						.SetString("SkillList", skillListString)
						.UniqueResult<int>();
				if (actualWorkloadInSeconds == -1)
					return null;

				return actualWorkloadInSeconds;
			}
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}
