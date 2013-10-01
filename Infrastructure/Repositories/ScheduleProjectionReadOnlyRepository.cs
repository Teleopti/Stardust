﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ScheduleProjectionReadOnlyRepository : IScheduleProjectionReadOnlyRepository
    {
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleProjectionReadOnlyRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
			_currentUnitOfWork = currentUnitOfWork;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1062:Validate arguments of public methods", MessageId = "1"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup,
                                                                      IScenario scenario)
        {
			var uow = _currentUnitOfWork.Current();

            return ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "exec ReadModel.LoadBudgetAllowanceReadModel @BudgetGroupId	= :budgetGroupId, @ScenarioId = :scenarioId, @DateFrom = :StartDate, @DateTo = :EndDate")
                                               .SetDateTime("StartDate", period.StartDate)
                                               .SetDateTime("EndDate", period.EndDate)
                                               .SetGuid("budgetGroupId", budgetGroup.Id.GetValueOrDefault())
                                               .SetGuid("scenarioId", scenario.Id.GetValueOrDefault())
                                               .SetResultTransformer(Transformers.AliasToBean(typeof (PayloadWorkTime)))
                                               .SetReadOnly(true)
                                               .List<PayloadWorkTime>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void ClearPeriodForPerson(DateOnlyPeriod period, Guid scenarioId, Guid personId)
        {
			var uow = _currentUnitOfWork.Current();
            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "DELETE FROM ReadModel.ScheduleProjectionReadOnly WHERE BelongsToDate BETWEEN :StartDate AND :EndDate AND ScenarioId=:scenario AND PersonId=:person")
                                        .SetGuid("person", personId)
                                        .SetGuid("scenario", scenarioId)
                                        .SetDateTime("StartDate", period.StartDate)
                                        .SetDateTime("EndDate", period.EndDate)
                                        .ExecuteUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId,
                                      ProjectionChangedEventLayer layer)
        {
			var uow = _currentUnitOfWork.Current();

            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "INSERT INTO ReadModel.ScheduleProjectionReadOnly (ScenarioId,PersonId,BelongsToDate,PayloadId,StartDateTime,EndDateTime,WorkTime,ContractTime,Name,ShortName,DisplayColor,PayrollCode,InsertedOn) VALUES (:ScenarioId,:PersonId,:Date,:PayloadId,:StartDateTime,:EndDateTime,:WorkTime,:ContractTime,:Name,:ShortName,:DisplayColor,:PayrollCode,:InsertedOn)")
                                        .SetGuid("ScenarioId", scenarioId)
                                        .SetGuid("PersonId", personId)
                                        .SetGuid("PayloadId", layer.PayloadId)
                                        .SetDateTime("StartDateTime", layer.StartDateTime)
                                        .SetDateTime("EndDateTime", layer.EndDateTime)
                                        .SetInt64("WorkTime", layer.WorkTime.Ticks)
                                        .SetInt64("ContractTime", layer.ContractTime.Ticks)
                                        .SetString("Name", layer.Name)
                                        .SetString("ShortName", layer.ShortName)
                                        .SetString("PayrollCode", string.Empty)
                                        .SetInt32("DisplayColor", layer.DisplayColor)
                                        .SetDateTime("Date", belongsToDate)
                                        .SetDateTime("InsertedOn", DateTime.UtcNow)
                                        .ExecuteUpdate();
        }

        public bool IsInitialized()
        {
			var uow = _currentUnitOfWork.Current();
            var result = ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "SELECT TOP 1 * FROM ReadModel.ScheduleProjectionReadOnly")
                                                     .List();
            return result.Count > 0;
        }

        public DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId)
        {
            var uow = _currentUnitOfWork.Current();
			var result = ((NHibernateUnitOfWork) uow).Session
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

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			var uow = _currentUnitOfWork.Current();

			return ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"SELECT PayloadId,StartDateTime,EndDateTime,WorkTime,Name,ShortName,DisplayColor,PayrollCode,ContractTime FROM ReadModel.ScheduleProjectionReadOnly WHERE ScenarioId=:ScenarioId AND PersonId=:PersonId AND BelongsToDate=:Date")
										.SetGuid("ScenarioId", scenarioId)
										.SetGuid("PersonId", personId)
										.SetDateTime("Date", date)
                                        .SetResultTransformer(new AliasToBeanResultTransformer(typeof(IntermediateProjectionChangedEventLayer)))
                                        .List<IntermediateProjectionChangedEventLayer>().Select(t => t.ToLayer());
	    }

        private class IntermediateProjectionChangedEventLayer
        {
            /// <summary>
            /// The payload id
            /// </summary>
            public Guid PayloadId { get; set; }
            /// <summary>
            /// The layer start time
            /// </summary>
            public DateTime StartDateTime { get; set; }
            /// <summary>
            /// The layer end time
            /// </summary>
            public DateTime EndDateTime { get; set; }
            /// <summary>
            /// The layer work time
            /// </summary>
            public long WorkTime { get; set; }
            /// <summary>
            /// The layer contract time
            /// </summary>
            public long ContractTime { get; set; }
            /// <summary>
            /// The name of the payload
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The short name of the payload
            /// </summary>
            public string ShortName { get; set; }
            /// <summary>
            /// The payroll code of the payload
            /// </summary>
            public string PayrollCode { get; set; }
            /// <summary>
            /// The display color of the payload
            /// </summary>
            public int DisplayColor { get; set; }
            /// <summary>
            /// Is this absence
            /// </summary>
            public bool IsAbsence { get; set; }
            /// <summary>
            /// Requires seat
            /// </summary>
            public bool RequiresSeat { get; set; }

            public ProjectionChangedEventLayer ToLayer()
            {
                return new ProjectionChangedEventLayer
                    {
                        ContractTime = TimeSpan.FromTicks(ContractTime),
                        WorkTime = TimeSpan.FromTicks(WorkTime),
                        DisplayColor = DisplayColor,
                        EndDateTime = DateTime.SpecifyKind(EndDateTime,DateTimeKind.Utc),
                        IsAbsence = IsAbsence,
                        Name = Name,
                        PayloadId = PayloadId,
                        PayrollCode = PayrollCode,
                        RequiresSeat = RequiresSeat,
                        ShortName = ShortName,
                        StartDateTime = DateTime.SpecifyKind(StartDateTime,DateTimeKind.Utc)
                    };
            }
        }

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnlyPeriod datePeriod, Guid personId, Guid scenarioId)
		{
			var uow = _currentUnitOfWork.Current();

			return ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
                "SELECT PayloadId,StartDateTime,EndDateTime,WorkTime,Name,ShortName,DisplayColor,PayrollCode,ContractTime FROM ReadModel.ScheduleProjectionReadOnly WHERE ScenarioId=:ScenarioId AND PersonId=:PersonId AND BelongsToDate>=:DateFrom AND BelongsToDate<=:DateTo")
										.SetGuid("ScenarioId", scenarioId)
										.SetGuid("PersonId", personId)
										.SetDateTime("DateFrom", datePeriod.StartDate)
										.SetDateTime("DateTo", datePeriod.EndDate)
										.SetResultTransformer(new AliasToBeanResultTransformer(typeof(IntermediateProjectionChangedEventLayer)))
										.List<IntermediateProjectionChangedEventLayer>().Select(t => t.ToLayer());
		}

	    public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
        {
            var uow = _currentUnitOfWork.Current();

            const string query = @"select sum(NumberOfRequests) as NumberOfRequests, BelongsToDate
                                    from (

                                            select COUNT(*) NumberOfRequests, t.BelongsToDate 
	                                        from (
	                                        SELECT distinct sp.*
	                                        from Absence a
	                                        inner join ReadModel.ScheduleProjectionReadOnly sp 
	                                        on a.Id = sp.PayloadId
	                                        inner join Person p
	                                        on sp.PersonId = p.Id
	                                        inner join PersonPeriod pp
                                            on pp.Parent = p.Id
	                                        and pp.BudgetGroup = :budgetGroupId
	                                        and sp.BelongsToDate = :currentDate 
	                                        ) t
	                                        group by t.BelongsToDate
	                                        union all
	                                        select 0 as 'NumberOfRequests',:currentDate as 'BelongsToDate'
	                                    ) a
                                    group by BelongsToDate";

            var queryResult = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(query)
                                                          .SetDateTime("currentDate", currentDate)
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
