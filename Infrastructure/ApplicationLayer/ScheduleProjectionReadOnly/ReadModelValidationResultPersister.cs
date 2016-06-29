using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly
{
	public class ReadModelValidationResultPersister : IReadModelValidationResultPersister
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ReadModelValidationResultPersister(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void SaveScheduleProjectionReadOnly(ReadModelValidationResult input)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(
				@"INSERT INTO [ReadModel].[ScheduleProjectionReadOnly_check] (PersonId, BelongsToDate, IsValid, UpdateOn) VALUES (:PersonId, :BelongsToDate, :IsValid, :UpdateOn)")
				.SetDateTime("BelongsToDate", input.Date)
				.SetGuid("PersonId", input.PersonId)
				.SetBoolean("IsValid", input.IsValid)
				.SetDateTime("UpdateOn", DateTime.UtcNow)
				.ExecuteUpdate();
		}

		public void SavePersonScheduleDay(ReadModelValidationResult input)
		{
			_currentUnitOfWork.Session()
				.CreateSQLQuery(
					@"INSERT INTO [ReadModel].[PersonScheduleDay_check] (PersonId, BelongsToDate, IsValid, UpdatedOn) VALUES (:PersonId, :BelongsToDate, :IsValid, :UpdatedOn)")
				.SetDateTime("BelongsToDate", input.Date)
				.SetGuid("PersonId", input.PersonId)
				.SetBoolean("IsValid", input.IsValid)
				.SetDateTime("UpdatedOn", DateTime.UtcNow)
				.ExecuteUpdate();
		}

		public void SaveScheduleDay(ReadModelValidationResult input)
		{
			_currentUnitOfWork.Session()
				.CreateSQLQuery(
					@"INSERT INTO [ReadModel].[ScheduleDay_check] (PersonId, BelongsToDate, IsValid, UpdatedOn) VALUES (:PersonId, :BelongsToDate, :IsValid, :UpdatedOn)")
				.SetDateTime("BelongsToDate",input.Date)
				.SetGuid("PersonId",input.PersonId)
				.SetBoolean("IsValid",input.IsValid)
				.SetDateTime("UpdatedOn",DateTime.UtcNow)
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

		public void Reset(IList<ValidateReadModelType> types)
		{
			if (types.Contains(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly_check]")
					.ExecuteUpdate();
			}

			if(types.Contains(ValidateReadModelType.PersonScheduleDay))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("TRUNCATE TABLE [ReadModel].[PersonScheduleDay_check]")
					.ExecuteUpdate();
			}

			if(types.Contains(ValidateReadModelType.ScheduleDay))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("TRUNCATE TABLE [ReadModel].[ScheduleDay_check]")
					.ExecuteUpdate();
			}
		}
	}
}
