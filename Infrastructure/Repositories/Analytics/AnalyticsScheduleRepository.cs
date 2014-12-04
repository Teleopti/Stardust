using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		
		public void PersistFactScheduleRow(IAnalyticsFactScheduleTime analyticsFactScheduleTime,
			AnalyticsFactScheduleDate analyticsFactScheduleDate, AnalyticsFactSchedulePerson personPart)
		{
			
		}

		public void PersistFactScheduleDayCountRow(AnalyticsFactScheduleDayCount dayCount)
		{
			
		}

		public IList<IAnalyticsActivity> Activities()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select activity_id ActivityId, activity_code ActivityCode, in_paid_time InPaidTime, in_ready_time InReadyTime from mart.dim_activity")
					.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsActivity)))
					.SetReadOnly(true)
					.List<IAnalyticsActivity>();
			}
		}

		public IList<IAnalyticsAbsence> Absences()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select absence_id AbsenceId, absence_code AbsenceCode, in_paid_time InPaidTime from mart.dim_absence")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsAbsence)))
					.SetReadOnly(true)
					.List<IAnalyticsAbsence>();
			}
		}

		private IUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
			return identity.DataSource.Statistic;
		}
	}

	public class AnalyticsActivity: IAnalyticsActivity
	{
		public int ActivityId { get; set; }
		public Guid ActivityCode { get; set; }
		public bool InPaidTime { get; set; }
		public bool InReadyTime { get; set; }
	}

	public class AnalyticsAbsence : IAnalyticsAbsence
	{
		public int AbsenceId { get; set; }
		public Guid AbsenceCode { get; set; }
		public bool InPaidTime { get; set; }
	}
}