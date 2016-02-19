using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Analytics.Tables;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			using (var connection = new SqlConnection(statisticUnitOfWorkFactory().ConnectionString))
			{
				var table = getTable(factScheduleRows);
				var adapter = new SqlDataAdapter
				{
					InsertCommand = new SqlCommand("mart.etl_fact_schedule_insert", connection)
					{
						CommandType = CommandType.StoredProcedure,
						UpdatedRowSource = UpdateRowSource.None
					}
				};

				adapter.InsertCommand.Parameters.Add("@shift_startdate_local_id", SqlDbType.Int, 4, table.Columns[0].ColumnName);
				adapter.InsertCommand.Parameters.Add("@schedule_date_id", SqlDbType.Int, 4, table.Columns[1].ColumnName);
				adapter.InsertCommand.Parameters.Add("@person_id", SqlDbType.Int, 4, table.Columns[2].ColumnName);
				adapter.InsertCommand.Parameters.Add("@interval_id", SqlDbType.SmallInt, 2, table.Columns[3].ColumnName);
				adapter.InsertCommand.Parameters.Add("@activity_starttime", SqlDbType.SmallDateTime, 4, table.Columns[4].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scenario_id", SqlDbType.Int, 4, table.Columns[5].ColumnName);
				adapter.InsertCommand.Parameters.Add("@activity_id", SqlDbType.Int, 4, table.Columns[6].ColumnName);
				adapter.InsertCommand.Parameters.Add("@absence_id", SqlDbType.Int, 4, table.Columns[7].ColumnName);
				adapter.InsertCommand.Parameters.Add("@activity_startdate_id", SqlDbType.Int, 4, table.Columns[8].ColumnName);
				adapter.InsertCommand.Parameters.Add("@activity_enddate_id", SqlDbType.Int, 4, table.Columns[9].ColumnName);
				adapter.InsertCommand.Parameters.Add("@activity_endtime", SqlDbType.SmallDateTime, 4, table.Columns[10].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_startdate_id", SqlDbType.Int, 4, table.Columns[11].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_starttime", SqlDbType.SmallDateTime, 4, table.Columns[12].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_enddate_id", SqlDbType.Int, 4, table.Columns[13].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_endtime", SqlDbType.SmallDateTime, 4, table.Columns[14].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_startinterval_id", SqlDbType.SmallInt, 2, table.Columns[15].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_endinterval_id", SqlDbType.SmallInt, 2, table.Columns[16].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_category_id", SqlDbType.Int, 4, table.Columns[17].ColumnName);
				adapter.InsertCommand.Parameters.Add("@shift_length_id", SqlDbType.Int, 4, table.Columns[18].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_time_m", SqlDbType.Int, 4, table.Columns[19].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_time_absence_m", SqlDbType.Int, 4, table.Columns[20].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_time_activity_m", SqlDbType.Int, 4, table.Columns[21].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_contract_time_m", SqlDbType.Int, 4, table.Columns[22].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_contract_time_activity_m", SqlDbType.Int, 4, table.Columns[23].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_contract_time_absence_m", SqlDbType.Int, 4, table.Columns[24].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_work_time_m", SqlDbType.Int, 4, table.Columns[25].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_work_time_activity_m", SqlDbType.Int, 4, table.Columns[26].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_work_time_absence_m", SqlDbType.Int, 4, table.Columns[27].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_over_time_m", SqlDbType.Int, 4, table.Columns[28].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_ready_time_m", SqlDbType.Int, 4, table.Columns[29].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_paid_time_m", SqlDbType.Int, 4, table.Columns[30].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_paid_time_activity_m", SqlDbType.Int, 4, table.Columns[31].ColumnName);
				adapter.InsertCommand.Parameters.Add("@scheduled_paid_time_absence_m", SqlDbType.Int, 4, table.Columns[32].ColumnName);
				adapter.InsertCommand.Parameters.Add("@business_unit_id", SqlDbType.Int, 4, table.Columns[33].ColumnName);
				adapter.InsertCommand.Parameters.Add("@datasource_update_date", SqlDbType.SmallDateTime, 4, table.Columns[37].ColumnName);
				adapter.InsertCommand.Parameters.Add("@overtime_id", SqlDbType.Int, 4, table.Columns[38].ColumnName);

				adapter.UpdateBatchSize = 20;

				connection.Open();
				adapter.Update(table);
			}
		}

		private DataTable getTable(IEnumerable<IFactScheduleRow> factScheduleRows)
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
					row.TimePart.OverTimeId);
			}

			return table;
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

		public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId, DateTime datasourceUpdateDate)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				uow.Session().CreateSQLQuery(@"exec mart.etl_stage_schedule_day_changed_servicebus_insert 
												@schedule_date_local=:Date,
												@person_code=:PersonId,
												@scenario_id=:ScenarioId,
												@business_unit_code=:BusinessUnitId,
												@datasource_update_date=:DatasourceUpdateDate")

					.SetDateTime("Date",date.Date)
					.SetGuid("PersonId", personId)
					.SetGuid("ScenarioId", scenarioId)
					.SetGuid("BusinessUnitId", businessUnitId)
					.SetDateTime("DatasourceUpdateDate", datasourceUpdateDate)
					.ExecuteUpdate();
			}
		}

		public IList<KeyValuePair<DateOnly, int>> Dates()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
                    "select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date BETWEEN DATEADD(DAY,-365, GETDATE()) AND  DATEADD(DAY, 365, GETDATE())")
					.SetResultTransformer(new CustomDictionaryTransformer()).List<KeyValuePair<DateOnly, int>>();
			}
		}

		public IList<IAnalyticsActivity> Activities()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
                    "select activity_id ActivityId, activity_code ActivityCode, in_paid_time InPaidTime, in_ready_time InReadyTime from mart.dim_activity WITH (NOLOCK)")
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
                    "select absence_id AbsenceId, absence_code AbsenceCode, in_paid_time InPaidTime from mart.dim_absence WITH (NOLOCK)")
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
                    "select scenario_id Id, scenario_code Code from mart.dim_scenario WITH (NOLOCK)")
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
                    "select shift_category_id Id, shift_category_code Code from mart.dim_shift_category WITH (NOLOCK)")
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
                    "select person_id PersonId, business_unit_id BusinessUnitId from mart.dim_person WITH (NOLOCK) WHERE person_period_code =:code ")
					.SetGuid("code", personPeriodCode)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsPersonBusinessUnit)))
					.SetReadOnly(true)
					//.SetTimeout(120)
					.UniqueResult<IAnalyticsPersonBusinessUnit>();
			}
		}


		public IList<IAnalyticsGeneric> Overtimes()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
                    "select overtime_id Id, overtime_code Code from mart.dim_overtime WITH (NOLOCK)")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGeneric)))
					.SetReadOnly(true)
					.List<IAnalyticsGeneric>();
			}
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
                    "select shift_length_id Id, shift_length_m ShiftLength from mart.dim_shift_length WITH (NOLOCK)")
					.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsShiftLength)))
					.SetReadOnly(true)
					.List<IAnalyticsShiftLength>();
			}
		}

		public int ShiftLengthId(int shiftLength)
		{
			using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(@"mart.etl_dim_shift_length_id_get @shift_length_m=:ShiftLength")
					.SetInt32("ShiftLength", shiftLength)
					.UniqueResult<int>();
			}
		}

		private IUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
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