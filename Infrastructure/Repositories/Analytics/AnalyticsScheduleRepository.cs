using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics.Tables;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
					getSqlDateCompatibleOrDefault(row.DatePart.ActivityStartTime, throwOnNotCompatible(nameof(row.DatePart.ActivityStartTime), row.DatePart.ActivityStartTime)),
					row.TimePart.ScenarioId,
					row.TimePart.ActivityId,
					row.TimePart.AbsenceId,
					row.DatePart.ActivityStartDateId,
					row.DatePart.ActivityEndDateId,
					getSqlDateCompatibleOrDefault(row.DatePart.ActivityEndTime, throwOnNotCompatible(nameof(row.DatePart.ActivityEndTime), row.DatePart.ActivityEndTime)),
					row.DatePart.ShiftStartDateId,
					getSqlDateCompatibleOrDefault(row.DatePart.ShiftStartTime, throwOnNotCompatible(nameof(row.DatePart.ShiftStartTime), row.DatePart.ShiftStartTime)),
					row.DatePart.ShiftEndDateId,
					getSqlDateCompatibleOrDefault(row.DatePart.ShiftEndTime, throwOnNotCompatible(nameof(row.DatePart.ShiftEndTime), row.DatePart.ShiftEndTime)),
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
					getSqlDateCompatibleOrDefault(row.DatePart.DatasourceUpdateDate, () => DateTime.UtcNow),
					row.TimePart.OverTimeId);
			}

			return table;
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_fact_schedule_day_count_insert] 
						@shift_startdate_local_id=:{nameof(dayCount.ShiftStartDateLocalId)},
						@person_id=:{nameof(dayCount.PersonId)}, 
						@business_unit_id=:{nameof(dayCount.BusinessUnitId)},
						@scenario_id=:{nameof(dayCount.ScenarioId)},
						@starttime=:{nameof(dayCount.StartTime)}, 
						@shift_category_id=:{nameof(dayCount.ShiftCategoryId)}, 
						@absence_id=:{nameof(dayCount.AbsenceId)}, 
						@day_off_name=:{nameof(dayCount.DayOffName)}, 
						@day_off_shortname=:{nameof(dayCount.DayOffShortName)}")
				.SetInt32(nameof(dayCount.ShiftStartDateLocalId), dayCount.ShiftStartDateLocalId)
				.SetInt32(nameof(dayCount.PersonId), dayCount.PersonId)
				.SetInt32(nameof(dayCount.BusinessUnitId), dayCount.BusinessUnitId)
				.SetInt32(nameof(dayCount.ScenarioId), dayCount.ScenarioId)
				.SetDateTime(nameof(dayCount.StartTime), dayCount.StartTime)
				.SetInt32(nameof(dayCount.ShiftCategoryId), dayCount.ShiftCategoryId)
				.SetInt32(nameof(dayCount.AbsenceId), dayCount.AbsenceId)
				.SetString(nameof(dayCount.DayOffName), dayCount.DayOffName)
				.SetString(nameof(dayCount.DayOffShortName), dayCount.DayOffShortName)

				.ExecuteUpdate();
		}

		public void DeleteFactSchedule(int dateId, Guid personCode, int scenarioId)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"mart.etl_fact_schedule_delete 
												@shift_startdate_local_id=:{nameof(dateId)}, 
												@person_code=:{nameof(personCode)}, 
												@scenario_id=:{nameof(scenarioId)}")
				.SetParameter(nameof(dateId), dateId)
				.SetParameter(nameof(personCode), personCode)
				.SetParameter(nameof(scenarioId), scenarioId)
				.ExecuteUpdate();
		}

		public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId, DateTime datasourceUpdateDate)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"exec mart.etl_stage_schedule_day_changed_servicebus_insert 
												@schedule_date_local=:{nameof(date)},
												@person_code=:{nameof(personId)},
												@scenario_id=:{nameof(scenarioId)},
												@business_unit_code=:{nameof(businessUnitId)},
												@datasource_update_date=:{nameof(datasourceUpdateDate)}")

				.SetDateTime(nameof(date), date.Date)
				.SetGuid(nameof(personId), personId)
				.SetGuid(nameof(scenarioId), scenarioId)
				.SetGuid(nameof(businessUnitId), businessUnitId)
				.SetDateTime(nameof(datasourceUpdateDate), getSqlDateCompatibleOrDefault(datasourceUpdateDate, () => DateTime.UtcNow))
				.ExecuteUpdate();
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			_analyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(
					$@"exec mart.etl_fact_schedule_update_unlinked_personids 
							@person_periodids=:PersonIds
							")
				.SetString("PersonIds", string.Join(",", personPeriodIds))
				.ExecuteUpdate();
		}

		public int GetFactScheduleRowCount(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select count(1) from mart.fact_schedule WITH (NOLOCK) WHERE person_id =:{nameof(personId)} ")
				.SetInt32(nameof(personId), personId)
				.UniqueResult<int>();
		}

		public int GetFactScheduleDayCountRowCount(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select count(1) from mart.fact_schedule_day_count WITH (NOLOCK) WHERE person_id =:{nameof(personId)} ")
				.SetInt32(nameof(personId), personId)
				.UniqueResult<int>();
		}

		public int GetFactScheduleDeviationRowCount(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select count(1) from mart.fact_schedule_deviation WITH (NOLOCK) WHERE person_id =:{nameof(personId)} ")
				.SetInt32(nameof(personId), personId)
				.UniqueResult<int>();
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT 
						shift_length_id {nameof(AnalyticsShiftLength.Id)}, 
						shift_length_m {nameof(AnalyticsShiftLength.ShiftLength)} 
					FROM mart.dim_shift_length WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsShiftLength)))
				.SetReadOnly(true)
				.List<IAnalyticsShiftLength>();
		}

		public int ShiftLengthId(int shiftLength)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"mart.etl_dim_shift_length_id_get @shift_length_m=:{nameof(shiftLength)}")
				.SetInt32(nameof(shiftLength), shiftLength)
				.UniqueResult<int>();
		}

		private static DateTime getSqlDateCompatibleOrDefault(DateTime input, Func<DateTime> defaultValue)
		{
			if (input < SqlDateTime.MinValue.Value || input > SqlDateTime.MaxValue.Value)
				return defaultValue();
			return input;
		}

		private static Func<DateTime> throwOnNotCompatible(string parameterName, DateTime value)
		{
			return () => { throw new ArgumentOutOfRangeException(parameterName, $"'{value}' is not valid to convert to a SqlDateTime"); };
		}
	}

	public class AnalyticsPersonBusinessUnit : IAnalyticsPersonBusinessUnit
	{
		public int PersonId { get; set; }
		public int BusinessUnitId { get; set; }
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