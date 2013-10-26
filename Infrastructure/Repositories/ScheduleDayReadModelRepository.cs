using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleDayReadModelRepository : IScheduleDayReadModelRepository
	{
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;

	    public ScheduleDayReadModelRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
		    _currentUnitOfWork = currentUnitOfWork;
		}

		public IList<ScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId)
		{
            return _currentUnitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, BelongsToDate AS Date, StartDateTime, EndDateTime, Workday, Label, DisplayColor AS ColorCode, WorkTime AS WorkTimeTicks, ContractTime AS ContractTimeTicks FROM ReadModel.ScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
				.SetGuid("personid", personId)
				.SetDateTime("startdate", startDate)
				.SetDateTime("enddate", toDate)
				.SetResultTransformer(Transformers.AliasToBean(typeof (ScheduleDayReadModel)))
				.SetReadOnly(true)
				.List<ScheduleDayReadModel>();
		}

		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
            _currentUnitOfWork.Session().CreateSQLQuery(
				"DELETE FROM ReadModel.ScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
				.SetGuid("person", personId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		public void SaveReadModel(ScheduleDayReadModel model)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
					"INSERT INTO ReadModel.ScheduleDay (PersonId,BelongsToDate,StartDateTime,EndDateTime,Workday,WorkTime,ContractTime,Label,DisplayColor,NotScheduled) VALUES (:PersonId,:Date,:StartDateTime,:EndDateTime,:Workday,:WorkTime,:ContractTime,:Label,:DisplayColor,:NotScheduled)")
					.SetGuid("PersonId", model.PersonId)
					.SetDateTime("StartDateTime", model.StartDateTime)
					.SetDateTime("EndDateTime", model.EndDateTime)
					.SetInt64("WorkTime", model.WorkTime.Ticks)
					.SetInt64("ContractTime", model.ContractTime.Ticks)
					.SetBoolean("Workday", model.Workday)
					.SetString("Label", model.Label)
					.SetInt32("DisplayColor", model.DisplayColor.ToArgb())
					.SetDateTime("Date", model.BelongsToDate)
					.SetBoolean("NotScheduled", model.NotScheduled)
					.ExecuteUpdate();
		}

		public bool IsInitialized()
		{
            var result = _currentUnitOfWork.Session().CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.ScheduleDay")
				.List();
			return result.Count > 0;
		}
	}
}