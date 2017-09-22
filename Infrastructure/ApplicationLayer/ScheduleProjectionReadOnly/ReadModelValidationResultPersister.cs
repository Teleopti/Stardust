using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

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

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleProjectionReadOnly()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				@"SELECT PersonId, BelongsToDate as [Date], IsValid FROM [ReadModel].[ScheduleProjectionReadOnly_check] WHERE IsValid = 0")
				.SetResultTransformer(Transformers.AliasToBean<ReadModelValidationResult>())
				.List<ReadModelValidationResult>();
			return result;
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidPersonScheduleDay()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				@"SELECT PersonId, BelongsToDate as [Date], IsValid FROM [ReadModel].[PersonScheduleDay_check] WHERE IsValid = 0")
				.SetResultTransformer(Transformers.AliasToBean<ReadModelValidationResult>())
				.List<ReadModelValidationResult>();
			return result;
		}

		public IEnumerable<ReadModelValidationResult> LoadAllInvalidScheduleDay()
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				@"SELECT PersonId, BelongsToDate as [Date], IsValid FROM [ReadModel].[ScheduleDay_check] WHERE IsValid = 0")
				.SetResultTransformer(Transformers.AliasToBean<ReadModelValidationResult>())
				.List<ReadModelValidationResult>();
			return result;
		}

		public void Reset(ValidateReadModelType types)
		{
			if (types.HasFlag(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("DELETE FROM [ReadModel].[ScheduleProjectionReadOnly_check]")
					.ExecuteUpdate();
			}

			if(types.HasFlag(ValidateReadModelType.PersonScheduleDay))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("DELETE FROM [ReadModel].[PersonScheduleDay_check]")
					.ExecuteUpdate();
			}

			if(types.HasFlag(ValidateReadModelType.ScheduleDay))
			{
				_currentUnitOfWork.Session().CreateSQLQuery("DELETE FROM [ReadModel].[ScheduleDay_check]")
					.ExecuteUpdate();
			}
		}
	}
}
