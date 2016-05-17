using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics.Tables;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsScheduleRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			var table = getTable(factScheduleRows);
			var insertCommand = new SqlCommand("mart.etl_fact_schedule_insert", (SqlConnection)_analyticsUnitOfWork.Current().Session().Connection)
			{
				CommandType = CommandType.StoredProcedure,
				UpdatedRowSource = UpdateRowSource.None,
				Parameters =
				{
					new SqlParameter("@shift_startdate_local_id", SqlDbType.Int, 4, table.Columns[0].ColumnName),
					new SqlParameter("@schedule_date_id", SqlDbType.Int, 4, table.Columns[1].ColumnName),
					new SqlParameter("@person_id", SqlDbType.Int, 4, table.Columns[2].ColumnName),
					new SqlParameter("@interval_id", SqlDbType.SmallInt, 2, table.Columns[3].ColumnName),
					new SqlParameter("@activity_starttime", SqlDbType.SmallDateTime, 4, table.Columns[4].ColumnName),
					new SqlParameter("@scenario_id", SqlDbType.Int, 4, table.Columns[5].ColumnName),
					new SqlParameter("@activity_id", SqlDbType.Int, 4, table.Columns[6].ColumnName),
					new SqlParameter("@absence_id", SqlDbType.Int, 4, table.Columns[7].ColumnName),
					new SqlParameter("@activity_startdate_id", SqlDbType.Int, 4, table.Columns[8].ColumnName),
					new SqlParameter("@activity_enddate_id", SqlDbType.Int, 4, table.Columns[9].ColumnName),
					new SqlParameter("@activity_endtime", SqlDbType.SmallDateTime, 4, table.Columns[10].ColumnName),
					new SqlParameter("@shift_startdate_id", SqlDbType.Int, 4, table.Columns[11].ColumnName),
					new SqlParameter("@shift_starttime", SqlDbType.SmallDateTime, 4, table.Columns[12].ColumnName),
					new SqlParameter("@shift_enddate_id", SqlDbType.Int, 4, table.Columns[13].ColumnName),
					new SqlParameter("@shift_endtime", SqlDbType.SmallDateTime, 4, table.Columns[14].ColumnName),
					new SqlParameter("@shift_startinterval_id", SqlDbType.SmallInt, 2, table.Columns[15].ColumnName),
					new SqlParameter("@shift_endinterval_id", SqlDbType.SmallInt, 2, table.Columns[16].ColumnName),
					new SqlParameter("@shift_category_id", SqlDbType.Int, 4, table.Columns[17].ColumnName),
					new SqlParameter("@shift_length_id", SqlDbType.Int, 4, table.Columns[18].ColumnName),
					new SqlParameter("@scheduled_time_m", SqlDbType.Int, 4, table.Columns[19].ColumnName),
					new SqlParameter("@scheduled_time_absence_m", SqlDbType.Int, 4, table.Columns[20].ColumnName),
					new SqlParameter("@scheduled_time_activity_m", SqlDbType.Int, 4, table.Columns[21].ColumnName),
					new SqlParameter("@scheduled_contract_time_m", SqlDbType.Int, 4, table.Columns[22].ColumnName),
					new SqlParameter("@scheduled_contract_time_activity_m", SqlDbType.Int, 4, table.Columns[23].ColumnName),
					new SqlParameter("@scheduled_contract_time_absence_m", SqlDbType.Int, 4, table.Columns[24].ColumnName),
					new SqlParameter("@scheduled_work_time_m", SqlDbType.Int, 4, table.Columns[25].ColumnName),
					new SqlParameter("@scheduled_work_time_activity_m", SqlDbType.Int, 4, table.Columns[26].ColumnName),
					new SqlParameter("@scheduled_work_time_absence_m", SqlDbType.Int, 4, table.Columns[27].ColumnName),
					new SqlParameter("@scheduled_over_time_m", SqlDbType.Int, 4, table.Columns[28].ColumnName),
					new SqlParameter("@scheduled_ready_time_m", SqlDbType.Int, 4, table.Columns[29].ColumnName),
					new SqlParameter("@scheduled_paid_time_m", SqlDbType.Int, 4, table.Columns[30].ColumnName),
					new SqlParameter("@scheduled_paid_time_activity_m", SqlDbType.Int, 4, table.Columns[31].ColumnName),
					new SqlParameter("@scheduled_paid_time_absence_m", SqlDbType.Int, 4, table.Columns[32].ColumnName),
					new SqlParameter("@business_unit_id", SqlDbType.Int, 4, table.Columns[33].ColumnName),
					new SqlParameter("@datasource_update_date", SqlDbType.SmallDateTime, 4, table.Columns[37].ColumnName),
					new SqlParameter("@overtime_id", SqlDbType.Int, 4, table.Columns[38].ColumnName)
				}
			};

			_analyticsUnitOfWork.Current().Session().Transaction.Enlist(insertCommand);

			var adapter = new SqlDataAdapter
			{
				InsertCommand = insertCommand,
				UpdateBatchSize = 20
			};

			adapter.Update(table);
		}

		private static DataTable getTable(IEnumerable<IFactScheduleRow> factScheduleRows)
		{
			var table = fact_schedule.CreateTable();

			foreach (var row in factScheduleRows)
			{
				table.AddFactSchedule(
					row.DatePart.ScheduleStartDateLocalId,
					row.DatePart.ScheduleDateId,
					row.PersonPart.PersonId,
					row.DatePart.IntervalId,
					row.DatePart.ActivityStartTime,
					row.TimePart.ScenarioId,
					row.TimePart.ActivityId,
					row.TimePart.AbsenceId,
					row.DatePart.ActivityStartDateId,
					row.DatePart.ActivityEndDateId,
					row.DatePart.ActivityEndTime,
					row.DatePart.ShiftStartDateId,
					row.DatePart.ShiftStartTime,
					row.DatePart.ShiftEndDateId,
					row.DatePart.ShiftEndTime,
					row.DatePart.ShiftStartIntervalId,
					row.DatePart.ShiftEndIntervalId,
					row.TimePart.ShiftCategoryId,
					row.TimePart.ShiftLengthId,
					row.TimePart.ScheduledMinutes,
					row.TimePart.ScheduledAbsenceMinutes,
					row.TimePart.ScheduledActivityMinutes,
					row.TimePart.ContractTimeMinutes,
					row.TimePart.ContractTimeActivityMinutes,
					row.TimePart.ContractTimeAbsenceMinutes,
					row.TimePart.WorkTimeMinutes,
					row.TimePart.WorkTimeActivityMinutes,
					row.TimePart.WorkTimeAbsenceMinutes,
					row.TimePart.OverTimeMinutes,
					row.TimePart.ReadyTimeMinues,
					row.TimePart.PaidTimeMinutes,
					row.TimePart.PaidTimeActivityMinutes,
					row.TimePart.PaidTimeAbsenceMinutes,
					row.PersonPart.BusinessUnitId,
					row.DatePart.DatasourceUpdateDate,
					row.TimePart.OverTimeId);
			}

			return table;
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(
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

		public void DeleteFactSchedule(int date, int personId, int scenarioId)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"mart.etl_fact_schedule_delete @shift_startdate_local_id=:DateId, @person_id=:PersonId, @scenario_id=:ScenarioId")
				.SetInt32("DateId", date)
				.SetInt32("PersonId", personId)
				.SetInt32("ScenarioId", scenarioId)
				.ExecuteUpdate();
		}

		public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId, DateTime datasourceUpdateDate)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"exec mart.etl_stage_schedule_day_changed_servicebus_insert 
												@schedule_date_local=:Date,
												@person_code=:PersonId,
												@scenario_id=:ScenarioId,
												@business_unit_code=:BusinessUnitId,
												@datasource_update_date=:DatasourceUpdateDate")

				.SetDateTime("Date", date.Date)
				.SetGuid("PersonId", personId)
				.SetGuid("ScenarioId", scenarioId)
				.SetGuid("BusinessUnitId", businessUnitId)
				.SetDateTime("DatasourceUpdateDate", datasourceUpdateDate)
				.ExecuteUpdate();
		}

		public IList<AnalyticsActivity> Activities()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select activity_id ActivityId, activity_code ActivityCode, in_paid_time InPaidTime, in_ready_time InReadyTime from mart.dim_activity WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsActivity)))
				.SetReadOnly(true)
				.List<AnalyticsActivity>();
		}

		public IList<IAnalyticsAbsence> Absences()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select absence_id AbsenceId, absence_code AbsenceCode, in_paid_time InPaidTime from mart.dim_absence WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsAbsence)))
				.SetReadOnly(true)
				.List<IAnalyticsAbsence>();
		}

		public IList<IAnalyticsGeneric> ShiftCategories()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select shift_category_id Id, shift_category_code Code from mart.dim_shift_category WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
				.SetReadOnly(true)
				.List<IAnalyticsGeneric>();
		}

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select person_id PersonId, business_unit_id BusinessUnitId from mart.dim_person WITH (NOLOCK) WHERE person_period_code =:code ")
				.SetGuid("code", personPeriodCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsPersonBusinessUnit)))
				.SetReadOnly(true)
				//.SetTimeout(120)
				.UniqueResult<IAnalyticsPersonBusinessUnit>();
		}


		public IList<IAnalyticsGeneric> Overtimes()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select overtime_id Id, overtime_code Code from mart.dim_overtime WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
				.SetReadOnly(true)
				.List<IAnalyticsGeneric>();
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select shift_length_id Id, shift_length_m ShiftLength from mart.dim_shift_length WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsShiftLength)))
				.SetReadOnly(true)
				.List<IAnalyticsShiftLength>();
		}

		public int ShiftLengthId(int shiftLength)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"mart.etl_dim_shift_length_id_get @shift_length_m=:ShiftLength")
				.SetInt32("ShiftLength", shiftLength)
				.UniqueResult<int>();
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

	public class AnalyticsAbsence : IAnalyticsAbsence
	{
		public int AbsenceId { get; set; }
		public Guid AbsenceCode { get; set; }
		public bool InPaidTime { get; set; }
	}

	public class AnalyticsShiftLength : IAnalyticsShiftLength
	{
		public int Id { get; set; }
		public int ShiftLength { get; set; }
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