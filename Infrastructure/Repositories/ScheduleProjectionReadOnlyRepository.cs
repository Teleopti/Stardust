using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleProjectionReadOnlyRepository : IScheduleProjectionReadOnlyRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ScheduleProjectionReadOnlyRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period,IBudgetGroup budgetGroup,IScenario scenario)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void ClearPeriodForPerson(DateOnlyPeriod period,Guid scenarioId,Guid personId)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
				"DELETE FROM ReadModel.ScheduleProjectionReadOnly WHERE BelongsToDate BETWEEN :StartDate AND :EndDate AND ScenarioId=:scenario AND PersonId=:person")
				.SetGuid("person", personId)
				.SetGuid("scenario", scenarioId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void AddProjectedLayer(DateOnly belongsToDate,Guid scenarioId,Guid personId, DenormalizedScheduleProjectionLayer layer)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			
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
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			var result = ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.ScheduleProjectionReadOnly")
				.List();
			return result.Count > 0;
		}

        public DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId)
        {
            var uow = _unitOfWorkFactory.CurrentUnitOfWork();
            string query = string.Format(CultureInfo.InvariantCulture,@"SELECT TOP 1 StartDateTime, EndDateTime 
                            FROM ReadModel.v_ScheduleProjectionReadOnlyRTA rta WHERE EndDateTime >= :endDate 
                            AND PersonId=:personId");

            var result = ((NHibernateUnitOfWork) uow).Session
                .CreateSQLQuery(query)
                .SetDateTime("endDate", dateTime)
                .SetGuid("personId", personId)
                .SetResultTransformer(Transformers.AliasToBean(typeof(ActivityPeriod)))
                .List<ActivityPeriod>();

	        if (result.Count < 1)
                return null;

	        var activityPeriod = result.First();
			DateTime? nextActivityDateTime = activityPeriod.StartDateTime > dateTime
				                                 ? activityPeriod.StartDateTime
				                                 : activityPeriod.EndDateTime;
	        return ((DateTime)nextActivityDateTime).ToLocalTime();
        }
	}

    public class ActivityPeriod
    {
		public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
