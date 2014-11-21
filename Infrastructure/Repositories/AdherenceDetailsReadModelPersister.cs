using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public AdherenceDetailsReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Add(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherenceDetails " +
				"(" +
				"	PersonId," +
				"	BelongsToDate," +
				"	Name," +
				"	StartTime," +
				"	ActualStartTime," +
				"	LastStateChangedTime," +
				"	IsInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ActivityHasEnded" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:Name," +
				"	:StartTime," +
				"	:ActualStartTime," +
				"	:LastStateChangedTime," +
				"	:IsInAdherence," +
				"	:TimeInAdherence," +
				"	:TimeOutOfAdherence," +
				"	:ActivityHasEnded" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("Name", model.Name)
				.SetParameter("StartTime", model.StartTime,NHibernateUtil.DateTime)
				.SetParameter("ActualStartTime", model.ActualStartTime, NHibernateUtil.DateTime)
				.SetParameter("LastStateChangedTime", model.LastStateChangedTime, NHibernateUtil.DateTime)
				.SetParameter("IsInAdherence", model.IsInAdherence)
				.SetTimeSpan("TimeInAdherence", model.TimeInAdherence)
				.SetTimeSpan("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ActivityHasEnded", model.ActivityHasEnded)
				.ExecuteUpdate();
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
			"UPDATE ReadModel.AdherenceDetails SET" +
			"			Name = :Name," +
			"			ActualStartTime = :ActualStartTime," +
			"			LastStateChangedTime = :LastStateChangedTime," +
			"			IsInAdherence = :IsInAdherence," +
			"			TimeInAdherence = :TimeInAdherence," +
			"			TimeOutOfAdherence = :TimeOutOfAdherence, " +
			"			ActivityHasEnded = :ActivityHasEnded " +
			"WHERE " +
			"	PersonId = :PersonId AND " +
			"	StartTime =:StartTime")
			.SetGuid("PersonId", model.PersonId)
			.SetParameter("Name", model.Name)
			.SetParameter("StartTime", model.StartTime)
			.SetParameter("ActualStartTime", model.ActualStartTime)
			.SetParameter("LastStateChangedTime", model.LastStateChangedTime)
			.SetParameter("IsInAdherence", model.IsInAdherence)
			.SetParameter("TimeInAdherence", model.TimeInAdherence)
			.SetParameter("TimeOutOfAdherence", model.TimeOutOfAdherence)
			.SetParameter("ActivityHasEnded", model.ActivityHasEnded)
			.ExecuteUpdate();
		}

		public IEnumerable<AdherenceDetailsReadModel> Get(Guid personId, DateOnly date)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date," +
				"	Name," +
				"	StartTime," +
				"	ActualStartTime," +
				"	LastStateChangedTime," +
				"	IsInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence, " +
				"	ActivityHasEnded " +
				"FROM ReadModel.AdherenceDetails WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("Name", NHibernateUtil.String)
				.AddScalar("StartTime", NHibernateUtil.DateTime)
				.AddScalar("ActualStartTime", NHibernateUtil.DateTime)
				.AddScalar("LastStateChangedTime", NHibernateUtil.DateTime)
				.AddScalar("IsInAdherence", NHibernateUtil.Boolean)
				.AddScalar("TimeInAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("TimeOutOfAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("ActivityHasEnded", NHibernateUtil.Boolean)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AdherenceDetailsReadModel)))
				.List<AdherenceDetailsReadModel>();
			return result;
		}

		public void Remove(Guid personId, DateOnly date)
		{
			_unitOfWork.Current().CreateSqlQuery(
			"DELETE FROM ReadModel.AdherenceDetails " +
			"WHERE " +
			"	PersonId = :PersonId AND " +
			"	BelongsToDate =:BelongsToDate")
			.SetGuid("PersonId", personId)
			.SetDateTime("BelongsToDate", date)
			.ExecuteUpdate();
		}
	}
}