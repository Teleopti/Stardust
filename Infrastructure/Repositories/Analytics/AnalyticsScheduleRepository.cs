using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Interfaces.Domain;
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

		public void DeleteFactSchedule(DateOnly date)
		{
			
		}

		public IList<KeyValuePair<DateOnly, int>> LoadDimDates(DateTime today)
		{
			return new List<KeyValuePair<DateOnly, int>>();
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

		public IList<IAnalyticsGeneric> Scenarios()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select scenario_id Id, scenario_code Code from mart.dim_scenario")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
					.SetReadOnly(true)
					.List<IAnalyticsGeneric>();
			}
		}

		public IList<IAnalyticsGeneric> ShiftCategories()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select shift_category_id Id, shift_category_code Code from mart.dim_shift_category")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
					.SetReadOnly(true)
					.List<IAnalyticsGeneric>();
			}
		}

		private IUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
			return identity.DataSource.Statistic;
		}
	}

	public class AnalyticsGeneric :IAnalyticsGeneric
	{
		public int Id { get; set; }
		public Guid Code { get; set; }
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