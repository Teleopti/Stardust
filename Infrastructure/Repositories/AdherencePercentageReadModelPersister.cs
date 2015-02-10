using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public AdherencePercentageReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Persist(AdherencePercentageReadModel model)
		{
			var existingReadModel = Get(model.BelongsToDate, model.PersonId);
			if (existingReadModel == null)
				saveReadModel(model);
			else
				updateReadModel(model);
		}

		private void updateReadModel(AdherencePercentageReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherencePercentage SET" +
				"			LastTimestamp = :LastTimestamp," +
				"			IsLastTimeInAdherence = :IsLastTimeInAdherence," +
				"			TimeInAdherence = :TimeInAdherence," +
				"			TimeOutOfAdherence = :TimeOutOfAdherence," +
				"			ShiftHasEnded = :ShiftHasEnded, " +
				"			[State] = :State " +
				"WHERE " +
				"	PersonId = :PersonId AND " +
				"	BelongsToDate =:Date")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.BelongsToDate)
				.SetParameter("LastTimestamp", model.LastTimestamp)
				.SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
				.SetParameter("TimeInAdherence", model.TimeInAdherence)
				.SetParameter("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ShiftHasEnded", model.ShiftHasEnded)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.ExecuteUpdate();
		}

		private void saveReadModel(AdherencePercentageReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherencePercentage " +
				"(" +
				"	PersonId," +
				"	BelongsToDate," +
				"	LastTimestamp," +
				"	IsLastTimeInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ShiftHasEnded," +
				"	[State]" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:LastTimestamp," +
				"	:IsLastTimeInAdherence," +
				"	:TimeInAdherence," +
				"	:TimeOutOfAdherence," +
				"	:ShiftHasEnded," +
				"	:State" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("LastTimestamp", model.LastTimestamp, NHibernateUtil.DateTime)
				.SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
				.SetTimeSpan("TimeInAdherence", model.TimeInAdherence)
				.SetTimeSpan("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ShiftHasEnded", model.ShiftHasEnded)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.ExecuteUpdate();
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date," +
				"	LastTimestamp," +
				"	IsLastTimeInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ShiftHasEnded, " +
				"	[State] AS StateJson " +
				"FROM ReadModel.AdherencePercentage WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("LastTimestamp", NHibernateUtil.DateTime)
				.AddScalar("IsLastTimeInAdherence", NHibernateUtil.Boolean)
				.AddScalar("TimeInAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("TimeOutOfAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("ShiftHasEnded", NHibernateUtil.Boolean)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List<internalModel>()
				.SingleOrDefault();

			if (result == null) return null;
			if (result.StateJson == null) return result;

			result.State = _deserializer.DeserializeObject<AdherencePercentageReadModelState[]>(result.StateJson);
			result.StateJson = null;

			return result;
		}

		public bool HasData()
		{
			return (int)_unitOfWork.Current().CreateSqlQuery("SELECT count(*) FROM ReadModel.AdherencePercentage ").UniqueResult() > 0;
		}

		private class internalModel : AdherencePercentageReadModel
		{
			public string StateJson { get; set; }
		} 

	}
}