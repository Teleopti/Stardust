using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly
{
    public class ScheduleProjectionReadOnlyPersister : IScheduleProjectionReadOnlyPersister
    {
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;

	    public ScheduleProjectionReadOnlyPersister(ICurrentUnitOfWork currentUnitOfWork)
	    {
		    _currentUnitOfWork = currentUnitOfWork;
	    }

	    public ScheduleProjectionReadOnlyPersister(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_currentUnitOfWork = new FromFactory(() =>unitOfWorkFactory);
		}

        public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup,
                                                                      IScenario scenario)
        {
            return _currentUnitOfWork.Session().CreateSQLQuery(
                "exec ReadModel.LoadBudgetAllowanceReadModel @BudgetGroupId	= :budgetGroupId, @ScenarioId = :scenarioId, @DateFrom = :StartDate, @DateTo = :EndDate")
											   .SetDateOnly("StartDate", period.StartDate)
											   .SetDateOnly("EndDate", period.EndDate)
                                               .SetGuid("budgetGroupId", budgetGroup.Id.GetValueOrDefault())
                                               .SetGuid("scenarioId", scenario.Id.GetValueOrDefault())
                                               .SetResultTransformer(Transformers.AliasToBean(typeof (PayloadWorkTime)))
                                               .SetReadOnly(true)
                                               .List<PayloadWorkTime>();
        }
		
	    public int ClearDayForPerson(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
        {
	        if (scheduleLoadedTimeStamp.Equals(DateTime.MinValue))
		        scheduleLoadedTimeStamp = DateTime.UtcNow;
			var count = _currentUnitOfWork.Session().CreateSQLQuery(
				"exec ReadModel.DeleteScheduleProjectionReadOnly @PersonId=:person, @ScenarioId=:scenario, @BelongsToDate=:date, @ScheduleLoadedTime=:scheduleLoadedTime")
                                        .SetGuid("person", personId)
                                        .SetGuid("scenario", scenarioId)
										.SetDateOnly("date", date)
										.SetDateTime("scheduleLoadedTime", scheduleLoadedTimeStamp)
										.UniqueResult<int>();

	        return count;
        }

		public int AddProjectedLayer(ScheduleProjectionReadOnlyModel model)
		{
			if (model.ScheduleLoadedTime.Equals(DateTime.MinValue))
				model.ScheduleLoadedTime = DateTime.UtcNow;
			return _currentUnitOfWork.Session().CreateSQLQuery(
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
					@InsertedOn =:InsertedOn,
					@ScheduleLoadedTime =:ScheduleLoadedTime")
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
				.SetDateTime("ScheduleLoadedTime", model.ScheduleLoadedTime)
				.UniqueResult<int>();
		}
		
        public bool IsInitialized()
        {
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
                "SELECT TOP 1 * FROM ReadModel.ScheduleProjectionReadOnly")
                                                     .List();
            return result.Count > 0;
        }

	    public IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
	    {
			return _currentUnitOfWork.Session().CreateSQLQuery(
				   "SELECT PersonId,ScenarioId,BelongsToDate,PayloadId,StartDateTime,EndDateTime,WorkTime,Name,ShortName,DisplayColor,ContractTime FROM ReadModel.ScheduleProjectionReadOnly WHERE ScenarioId=:ScenarioId AND PersonId=:PersonId AND BelongsToDate=:Date")
										   .SetGuid("ScenarioId", scenarioId)
										   .SetGuid("PersonId", personId)
										   .SetDateOnly("Date", date)
										   .SetResultTransformer(new AliasToBeanResultTransformer(typeof(internalModel)))
										   .List<internalModel>();
		}

	    public DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId)
        {
           var result = _currentUnitOfWork.Session()
				.CreateSQLQuery("exec ReadModel.GetNextActivityStartTime @PersonId=:personId, @UtcNow=:dateTime")
				.SetDateTime("dateTime", dateTime)
                .SetGuid("personId", personId)
                .SetResultTransformer(Transformers.AliasToBean(typeof(ActivityPeriod)))
                .List<ActivityPeriod>();

	        if (result.Count < 1)
                return null;

	        var activityPeriod = result.First();
			DateTime nextActivityDateTime = activityPeriod.StartDateTime > dateTime
				                                 ? activityPeriod.StartDateTime
				                                 : activityPeriod.EndDateTime;
	        return nextActivityDateTime;
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
                                            and a.Requestable = 1
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

            var queryResult = _currentUnitOfWork.Session().CreateSQLQuery(query)
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

	public class ActivityPeriod
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

	public class AbsenceRequestInfo
    {
        public int NumberOfRequests { get; set; }
        public DateTime BelongsToDate { get; set; }
    }
}
