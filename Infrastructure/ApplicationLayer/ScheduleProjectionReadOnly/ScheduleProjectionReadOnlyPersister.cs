using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly
{
	public class ScheduleProjectionReadOnlyPersister : IScheduleProjectionReadOnlyPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public ScheduleProjectionReadOnlyPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		
		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup,
																	  IScenario scenario)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"exec ReadModel.LoadBudgetAllowanceReadModel @BudgetGroupId	= :budgetGroupId, @ScenarioId = :scenarioId, @DateFrom = :StartDate, @DateTo = :EndDate")
				.SetDateOnly("StartDate", period.StartDate)
				.SetDateOnly("EndDate", period.EndDate)
				.SetGuid("budgetGroupId", budgetGroup.Id.GetValueOrDefault())
				.SetGuid("scenarioId", scenario.Id.GetValueOrDefault())
				.SetResultTransformer(Transformers.AliasToBean(typeof (PayloadWorkTime)))
				.SetReadOnly(true)
				.List<PayloadWorkTime>();
		}
		
		public bool BeginAddingSchedule(DateOnly date, Guid scenarioId, Guid personId,  int version)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"exec ReadModel.DeleteScheduleProjectionReadOnly @PersonId=:person, @ScenarioId=:scenario, @BelongsToDate=:date, @Version=:version")
				.SetGuid("person", personId)
				.SetGuid("scenario", scenarioId)
				.SetDateOnly("date", date)
				.SetInt32("version", version)
				.UniqueResult<bool>();
		}

		public void AddActivity(IEnumerable<ScheduleProjectionReadOnlyModel> models)
		{
			var connectionString = _unitOfWork.Current().Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var dt = new DataTable();
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.ScenarioId), typeof(Guid));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.PersonId), typeof(Guid));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.BelongsToDate), typeof(DateTime));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.PayloadId), typeof(Guid));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.StartDateTime), typeof(DateTime));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.EndDateTime), typeof(DateTime));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.WorkTime), typeof(long));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.Name), typeof(String));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.ShortName), typeof(String));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.DisplayColor), typeof(int));
				dt.Columns.Add("PayrollCode", typeof(String));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				dt.Columns.Add(nameof(ScheduleProjectionReadOnlyModel.ContractTime), typeof(long));

				var insertedOn = DateTime.UtcNow;

				using (var transaction = connection.BeginTransaction())
				{
					foreach (var model in models)
					{
						var row = dt.NewRow();

						row["ScenarioId"] = model.ScenarioId;
						row["PersonId"] = model.PersonId;
						row["BelongsToDate"] = model.BelongsToDate.Date;
						row["PayloadId"] = model.PayloadId;
						row["StartDateTime"] = model.StartDateTime;
						row["EndDateTime"] = model.EndDateTime;
						row["WorkTime"] = model.WorkTime.Ticks;
						row["Name"] = model.Name;
						row["ShortName"] = model.ShortName;
						row["DisplayColor"] = model.DisplayColor;
						row["PayrollCode"] = "";
						row["InsertedOn"] = insertedOn;
						row["ContractTime"] = model.ContractTime.Ticks;

						dt.Rows.Add(row);
					}

					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[ScheduleProjectionReadOnly]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}
			}
		}
		
		public bool IsInitialized()
		{
			var result = _unitOfWork.Session().CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.ScheduleProjectionReadOnly")
													 .List();
			return result.Count > 0;
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				   "SELECT PersonId,ScenarioId,BelongsToDate,PayloadId,StartDateTime,EndDateTime,WorkTime,Name,ShortName,DisplayColor,ContractTime FROM ReadModel.ScheduleProjectionReadOnly WHERE ScenarioId=:ScenarioId AND PersonId=:PersonId AND BelongsToDate=:Date")
										   .SetGuid("ScenarioId", scenarioId)
										   .SetGuid("PersonId", personId)
										   .SetDateOnly("Date", date)
										   .SetResultTransformer(new AliasToBeanResultTransformer(typeof(ScheduleProjectionReadOnlyPersister.internalModel)))
										   .List<ScheduleProjectionReadOnlyPersister.internalModel>();
		}

		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			const string query = @"select sum(NumberOfRequests) as NumberOfRequests, BelongsToDate
									from (
											select COUNT(*) NumberOfRequests, t.BelongsToDate 
											from (
											SELECT distinct sp.PersonId, sp.BelongsToDate
											from Absence a
											inner join ReadModel.ScheduleProjectionReadOnly sp 
											on a.Id = sp.PayloadId
											inner join BudgetAbsenceCollection bd
											on bd.Absence = a.Id
											inner join Person p
											on sp.PersonId = p.Id
											inner join PersonPeriod pp
											on pp.Parent = p.Id
											and pp.BudgetGroup = :budgetGroupId
											and :currentDate BETWEEN pp.StartDate and pp.EndDate
											and sp.BelongsToDate = :currentDate 
											) t
											group by t.BelongsToDate
											union all
											select 0 as 'NumberOfRequests',:currentDate as 'BelongsToDate'
										) a
									group by BelongsToDate";

			var queryResult = _unitOfWork.Session().CreateSQLQuery(query)
														  .SetDateOnly("currentDate", currentDate)
														  .SetGuid("budgetGroupId", budgetGroupId)
														  .SetResultTransformer(Transformers.AliasToBean(typeof(AbsenceRequestInfo)))
														  .List<AbsenceRequestInfo>();
			int numberOfHeadCounts = 0;

			if (queryResult.Count > 0)
			{
				var absenceRequestInfo = queryResult[0];
				numberOfHeadCounts = absenceRequestInfo.NumberOfRequests;
			}

			return numberOfHeadCounts;
		}
		
		private class internalModel : ScheduleProjectionReadOnlyModel
		{
			public new long WorkTime { set { base.WorkTime = TimeSpan.FromTicks(value); } }
			public new long ContractTime { set { base.ContractTime = TimeSpan.FromTicks(value); } }
			public new DateTime BelongsToDate { set { base.BelongsToDate = new DateOnly(value); } }
		}
		
	}

	public class AbsenceRequestInfo
	{
		public int NumberOfRequests { get; set; }
		public DateTime BelongsToDate { get; set; }
	}
}

