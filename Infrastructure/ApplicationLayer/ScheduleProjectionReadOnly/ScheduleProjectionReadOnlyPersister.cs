using System;
using System.Collections.Generic;
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

		public void AddActivity(ScheduleProjectionReadOnlyModel model)
		{
			_unitOfWork.Session().CreateSQLQuery(
				@"exec ReadModel.UpdateScheduleProjectionReadOnly 
					@PersonId=:PersonId,
					@ScenarioId =:ScenarioId,
					@BelongsToDate =:Date,
					@PayloadId =:PayloadId,
					@StartDateTime =:StartDateTime,
					@EndDateTime =:EndDateTime,
					@WorkTime =:WorkTime,
					@ContractTime =:ContractTime,
					@Name =:Name,
					@ShortName =:ShortName,
					@DisplayColor =:DisplayColor,
					@PayrollCode = '',
					@InsertedOn =:InsertedOn")
				.SetGuid("PersonId", model.PersonId)
				.SetGuid("ScenarioId", model.ScenarioId)
				.SetDateOnly("Date", model.BelongsToDate)
				.SetGuid("PayloadId", model.PayloadId)
				.SetInt64("WorkTime", model.WorkTime.Ticks)
				.SetInt64("ContractTime", model.ContractTime.Ticks)
				.SetDateTime("StartDateTime", model.StartDateTime)
				.SetDateTime("EndDateTime", model.EndDateTime)
				.SetString("Name", model.Name)
				.SetString("ShortName", model.ShortName)
				.SetInt32("DisplayColor", model.DisplayColor)
				.SetDateTime("InsertedOn", DateTime.UtcNow)
				.ExecuteUpdate();
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
											from Absence a WITH (NOLOCK)
											inner join ReadModel.ScheduleProjectionReadOnly sp 
											on a.Id = sp.PayloadId
											inner join BudgetAbsenceCollection bd WITH (NOLOCK)
											on bd.Absence = a.Id
											inner join Person p WITH (NOLOCK)
											on sp.PersonId = p.Id
											inner join PersonPeriod pp WITH (NOLOCK)
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

