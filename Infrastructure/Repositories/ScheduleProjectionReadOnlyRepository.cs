using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		public void ClearPeriodForPerson(DateOnlyPeriod period,IScenario scenario,Guid personId)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
				"DELETE FROM ReadModel.ScheduleProjectionReadOnly WHERE BelongsToDate BETWEEN :StartDate AND :EndDate AND ScenarioId=:scenario AND PersonId=:person")
				.SetGuid("person", personId)
				.SetGuid("scenario", scenario.Id.GetValueOrDefault())
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void AddProjectedLayer(DateOnly belongsToDate,IScenario scenario,Guid personId, IVisualLayer visualLayer)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			var description = visualLayer.DisplayDescription();

			var visualLayerImp = visualLayer as VisualLayer;
			var contractTime = visualLayerImp==null ? 0 : visualLayerImp.ContractTime().Ticks;

			((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
				"INSERT INTO ReadModel.ScheduleProjectionReadOnly (ScenarioId,PersonId,BelongsToDate,PayloadId,StartDateTime,EndDateTime,WorkTime,ContractTime,Name,ShortName,DisplayColor,PayrollCode,InsertedOn) VALUES (:ScenarioId,:PersonId,:Date,:PayloadId,:StartDateTime,:EndDateTime,:WorkTime,:ContractTime,:Name,:ShortName,:DisplayColor,:PayrollCode,:InsertedOn)")
				.SetGuid("ScenarioId", scenario.Id.GetValueOrDefault())
				.SetGuid("PersonId", personId)
				.SetGuid("PayloadId", visualLayer.Payload.UnderlyingPayload.Id.GetValueOrDefault())
				.SetDateTime("StartDateTime", visualLayer.Period.StartDateTime)
				.SetDateTime("EndDateTime", visualLayer.Period.EndDateTime)
				.SetInt64("WorkTime", visualLayer.WorkTime().Ticks)
				.SetInt64("ContractTime", contractTime)
				.SetString("Name", description.Name)
				.SetString("ShortName", description.ShortName)
				.SetString("PayrollCode", string.Empty)
				.SetInt32("DisplayColor", visualLayer.DisplayColor().ToArgb())
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
	}
}
