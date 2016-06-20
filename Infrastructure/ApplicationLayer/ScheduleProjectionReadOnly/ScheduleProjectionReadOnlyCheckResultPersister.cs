using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly
{
	public class ScheduleProjectionReadOnlyCheckResultPersister : IScheduleProjectionReadOnlyCheckResultPersister
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleProjectionReadOnlyCheckResultPersister(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void Save(ScheduleProjectionReadOnlyValidationResult input)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				@"INSERT INTO [CheckReadModel].[ScheduleProjectionReadOnly] (PersonId, BelongsToDate, IsValid, UpdateOn) VALUES (:PersonId, :BelongsToDate, :IsValid, :UpdateOn)")
				.SetDateTime("BelongsToDate", input.Date)
				.SetGuid("PersonId", input.PersonId)
				.SetBoolean("IsValid", input.IsValid)
				.SetDateTime("UpdateOn", DateTime.UtcNow)
				.ExecuteUpdate();
		}

		public void Reset()
		{
			_currentUnitOfWork.Session().CreateSQLQuery("TRUNCATE TABLE [CheckReadModel].[ScheduleProjectionReadOnly]")
				.ExecuteUpdate();
		}
	}
}
