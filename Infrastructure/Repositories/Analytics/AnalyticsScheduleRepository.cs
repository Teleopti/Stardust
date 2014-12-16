using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{

		public void PersistFactScheduleRow(IAnalyticsFactScheduleTime timePart,
			IAnalyticsFactScheduleDate datePart, IAnalyticsFactSchedulePerson personPart)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				uow.Session().CreateSQLQuery(
					@"exec mart.[etl_fact_schedule_insert] @shift_startdate_local_id=:LocalId, @schedule_date_id=:ScheduleDateId, 
@person_id=:PersonId, @interval_id=:IntervalId, @activity_starttime=:ActivityStarttime, @scenario_id=:ScenarioId,
@activity_id=:ActivityId, @absence_id=:AbsenceId, @activity_startdate_id=:ActivityStartdateId, @activity_enddate_id=:ActivityEnddateId,
@activity_endtime=:ActivityEndtime, @shift_startdate_id=:ShiftStartdateId, @shift_starttime=:ShiftStarttime, @shift_enddate_id=:ShiftEnddateId,
@shift_endtime=:ShiftEndtime, @shift_startinterval_id=:ShiftStartintervalId, @shift_endinterval_id=:ShiftEndintervalId,
@shift_category_id=:ShiftCategoryId, @shift_length_m=:ShiftLength, @scheduled_time_m=:ScheduledTimeM, @scheduled_time_absence_m=:ScheduledTimeAbsence,
@scheduled_time_activity_m=:ScheduledTimeActivity, @scheduled_contract_time_m=:ContractTime, @scheduled_contract_time_activity_m=:ContractTimeActivity,
@scheduled_contract_time_absence_m=:ContractTimeAbsence, @scheduled_work_time_m=:WorkTime, @scheduled_work_time_activity_m=:WorkTimeActivity,
@scheduled_work_time_absence_m=:WorkTimeAbsence, @scheduled_over_time_m=:OverTime, @scheduled_ready_time_m=:ReadyTime,
@scheduled_paid_time_m=:PaidTime, @scheduled_paid_time_activity_m=:PaidTimeActivity, @scheduled_paid_time_absence_m=:PaidTimeAbsence,
@business_unit_id=:BusinessUnitId, @datasource_update_date=:UpdateDate, @overtime_id=:OvertimeId")
					.SetInt32("LocalId", datePart.ScheduleStartDateLocalId)
					.SetInt32("ScheduleDateId", datePart.ScheduleDateId)
					.SetInt32("PersonId", personPart.PersonId)
					.SetInt32("IntervalId", datePart.IntervalId)
					.SetDateTime("ActivityStarttime", datePart.ActivityStartTime)
					.SetInt32("ScenarioId", timePart.ScenarioId)
					.SetInt32("ActivityId", timePart.ActivityId)
					.SetInt32("AbsenceId", timePart.AbsenceId)
					.SetInt32("ActivityStartdateId", datePart.ActivityStartDateId)
					.SetInt32("ActivityEnddateId", datePart.ActivityEndDateId)
					.SetDateTime("ActivityEndtime", datePart.ActivityEndTime)
					.SetInt32("ShiftStartdateId", datePart.ShiftStartDateId)
					.SetDateTime("ActivityEndtime", datePart.ActivityEndTime)
					.SetDateTime("ShiftStarttime", datePart.ShiftStartTime)
					.SetInt32("ShiftEnddateId", datePart.ShiftEndDateId)
					.SetDateTime("ShiftEndtime", datePart.ShiftEndTime)
					.SetInt32("ShiftStartintervalId", datePart.ShiftStartIntervalId)
					.SetInt32("ShiftEndintervalId", datePart.ShiftEndIntervalId)
					.SetInt32("ShiftCategoryId", timePart.ShiftCategoryId)
					.SetInt32("ShiftLength", timePart.ShiftLength)
					.SetInt32("ScheduledTimeM", timePart.ScheduledMinutes)
					.SetInt32("ScheduledTimeAbsence", timePart.ScheduledAbsenceMinutes)
					.SetInt32("ScheduledTimeActivity", timePart.ScheduledActivityMinutes)
					.SetInt32("ContractTime", timePart.ContractTimeMinutes)
					.SetInt32("ContractTimeActivity", timePart.ContractTimeActivityMinutes)
					.SetInt32("ContractTimeAbsence", timePart.ContractTimeAbsenceMinutes)
					.SetInt32("WorkTime", timePart.WorkTimeMinutes)
					.SetInt32("WorkTimeActivity", timePart.WorkTimeActivityMinutes)
					.SetInt32("WorkTimeAbsence", timePart.WorkTimeAbsenceMinutes)
					.SetInt32("OverTime", timePart.OverTimeMinutes)
					.SetInt32("ReadyTime", timePart.ReadyTimeMinues)
					.SetInt32("PaidTime", timePart.PaidTimeMinutes)
					.SetInt32("PaidTimeActivity", timePart.PaidTimeActivityMinutes)
					.SetInt32("PaidTimeAbsence", timePart.PaidTimeAbsenceMinutes)
					.SetInt32("BusinessUnitId", personPart.BusinessUnitId)
					.SetDateTime("UpdateDate", DateTime.Now)
					.SetInt32("OvertimeId", timePart.OverTimeId)

					.ExecuteUpdate();
			}
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				uow.Session().CreateSQLQuery(
					@"exec mart.[etl_fact_schedule_day_count_insert] 
						@shift_startdate_local_id=:LocalId,
						@person_id=:PersonId, 
						@business_unit_id=:BusinessUnitId,
						@scenario_id=:ScenarioId,
						@starttime=:Starttime, 
						@shift_category_id=:ShiftCategoryId, 
						@absence_id=:AbsenceId, 
						@day_off_name=:DayOffName, 
						@day_off_shortname=:DayOffShortName")
					.SetInt32("LocalId", dayCount.ShiftStartDateLocalId)
					.SetInt32("PersonId", dayCount.PersonId)
					.SetInt32("BusinessUnitId", dayCount.BusinessUnitId)
					.SetInt32("ScenarioId", dayCount.ScenarioId)
					.SetDateTime("Starttime", dayCount.StartTime)
					.SetInt32("ShiftCategoryId", dayCount.ShiftCategoryId)
					.SetInt32("AbsenceId", dayCount.AbsenceId)
					.SetString("DayOffName", dayCount.DayOffName)
					.SetString("DayOffShortName", dayCount.DayOffShortName)

					.ExecuteUpdate();
			}
		}

		public void DeleteFactSchedule(int date, int personId, int scenarioId)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				uow.Session().CreateSQLQuery(@"mart.etl_fact_schedule_delete @shift_startdate_local_id=:DateId, @person_id=:PersonId, @scenario_id=:ScenarioId")
					.SetInt32("DateId", date)
					.SetInt32("PersonId", personId)
					.SetInt32("ScenarioId", scenarioId)
					.ExecuteUpdate();
			}
		}

		public IList<KeyValuePair<DateOnly, int>> Dates()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select date_id, date_date from mart.dim_date where date_date BETWEEN DATEADD(DAY,-365, GETDATE()) AND  DATEADD(DAY, 365, GETDATE())")
					.SetResultTransformer(new CustomDictionaryTransformer()).List<KeyValuePair<DateOnly, int>>();
			}
		}

		public IList<IAnalyticsActivity> Activities()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select activity_id ActivityId, activity_code ActivityCode, in_paid_time InPaidTime, in_ready_time InReadyTime from mart.dim_activity")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
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

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select person_id PersonId, business_unit_id BusinessUnitId from mart.dim_person WHERE person_period_code =:code")
					.SetGuid("code", personPeriodCode)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsPersonBusinessUnit)))
					.SetReadOnly(true)
					.UniqueResult<IAnalyticsPersonBusinessUnit>();
			}
		}

		private IUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
			return identity.DataSource.Statistic;
		}
	}

	public class AnalyticsPersonBusinessUnit : IAnalyticsPersonBusinessUnit
	{
		public int PersonId { get; set; }
		public int BusinessUnitId { get; set; }
	}

	public class AnalyticsGeneric : IAnalyticsGeneric
	{
		public int Id { get; set; }
		public Guid Code { get; set; }
	}

	public class AnalyticsActivity : IAnalyticsActivity
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

	public class CustomDictionaryTransformer : IResultTransformer
	{
		public object TransformTuple(object[] tuple, string[] aliases)
		{
			int id = 0;
			var key = new DateOnly();
			for (int i = 0; i < tuple.Length; i++)
			{
				string alias = aliases[i];
				if (alias == "date_date") key = new DateOnly((DateTime)tuple[i]);
				else id = (int)tuple[i];
			}

			return new KeyValuePair<DateOnly, int>(key, id);
		}

		public IList TransformList(IList collection)
		{
			return collection;
		}
	}
}