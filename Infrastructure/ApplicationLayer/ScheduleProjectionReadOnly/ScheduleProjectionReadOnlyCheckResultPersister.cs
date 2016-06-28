using System;
using System.Collections.Generic;
using NHibernate.Transform;
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

		public void Save(ReadModelValidationResult input)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				@"INSERT INTO [ReadModel].[ScheduleProjectionReadOnly_check] (PersonId, BelongsToDate, IsValid, UpdateOn) VALUES (:PersonId, :BelongsToDate, :IsValid, :UpdateOn)")
				.SetDateTime("BelongsToDate", input.Date)
				.SetGuid("PersonId", input.PersonId)
				.SetBoolean("IsValid", input.IsValid)
				.SetDateTime("UpdateOn", DateTime.UtcNow)
				.ExecuteUpdate();
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalid()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				@"SELECT PersonId, BelongsToDate as [Date], IsValid FROM [ReadModel].[ScheduleProjectionReadOnly_check] WHERE IsValid = 0")
				.SetResultTransformer(Transformers.AliasToBean<ReadModelValidationResult>())
				.List<ReadModelValidationResult>();
			return result;
		}

		public void Reset()
		{
			_currentUnitOfWork.Session().CreateSQLQuery("TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly_check]")
				.ExecuteUpdate();
		}
	}
}
