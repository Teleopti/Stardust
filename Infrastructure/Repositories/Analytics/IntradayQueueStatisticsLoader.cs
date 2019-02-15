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
	public class IntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		public IList<SkillIntervalStatistics> LoadSkillVolumeStatistics(IList<ISkill> skills, DateTime startOfDayUtc)
		{
			var skillIdArray = skills.Select(x => x.Id.Value.ToString()).ToArray();
			var endOfDayUtc = startOfDayUtc.AddDays(1);

			var datesToLoad = Enumerable.Range(0, 1 + endOfDayUtc.Subtract(startOfDayUtc).Days)
				.Select(offset => startOfDayUtc.AddDays(offset))
				.ToArray();

			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var skillIdString = String.Join(",", skillIdArray);
				var allIntervals = new List<SkillIntervalStatistics>();

				foreach (var day in datesToLoad)
				{
					var callsPerSkillInterval =
						uow.Session()
							.CreateSQLQuery(
								@"mart.web_intraday_calls_per_skill_interval @today=:Today, @skill_list=:SkillList")
							.AddScalar("SkillId", NHibernateUtil.Guid)
							.AddScalar("WorkloadId", NHibernateUtil.Guid)
							.AddScalar("StartTime", NHibernateUtil.DateTime)
							.AddScalar("Calls", NHibernateUtil.Double)
							.AddScalar("AverageHandleTime", NHibernateUtil.Double)
							.AddScalar("AnsweredCalls", NHibernateUtil.Int32)
							.AddScalar("HandleTime", NHibernateUtil.Double)
							.SetString("Today", day.ToString("d", (CultureInfo.InvariantCulture)))
							.SetParameter("SkillList", skillIdString, NHibernateUtil.StringClob)
							.SetResultTransformer(Transformers.AliasToBean(typeof(SkillIntervalStatistics)))
							.List<SkillIntervalStatistics>();

					allIntervals.AddRange(callsPerSkillInterval);
				}

				return allIntervals.Where(x => x.StartTime >= startOfDayUtc).ToList();
			}
		}

		public int LoadActualEmailBacklogForWorkload(Guid workloadId, DateTimePeriod closedPeriod)
		{
			var startDate = closedPeriod.StartDateTime.Date;
			var endDate = closedPeriod.EndDateTime.Date;
			var startTime = new DateTime(1900, 01, 01).Add(closedPeriod.StartDateTime.TimeOfDay);
			var endTime = new DateTime(1900, 01, 01).Add(closedPeriod.EndDateTime.TimeOfDay);

			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var emailsPerWorkload =
					uow.Session()
						.CreateSQLQuery(
							@"mart.web_intraday_email_backlog_per_workload @workload_id=:WorkloadId, @start_date=:StartDate, @start_time=:StartTime, @end_date=:EndDate, @end_time=:EndTime")
						.AddScalar("Emails", NHibernateUtil.Int32)
						.SetGuid("WorkloadId", workloadId)
						.SetDateTime("StartDate", startDate)
						.SetDateTime("StartTime", startTime)
						.SetDateTime("EndDate", endDate)
						.SetDateTime("EndTime", endTime)
						.UniqueResult<int>();

				return emailsPerWorkload;
			}
		}

		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}
