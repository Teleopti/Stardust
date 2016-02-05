using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class AdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister, IAdherencePercentageReadModelReader
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
			var fromVersion = model.Version;
			var toVersion = model.Version + 1;
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE INTO ReadModel.AdherencePercentage AS T " +
				"USING (VALUES (:PersonId, :Date, :FromVersion)) AS S (PersonId, Date, [Version]) " +
				"ON T.PersonId = S.PersonId AND T.BelongsToDate = S.Date AND T.[Version] = S.[Version] " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		PersonId," +
				"		BelongsToDate," +
				"		[Version]," +
				"		LastTimestamp," +
				"		IsLastTimeInAdherence," +
				"		TimeInAdherence," +
				"		TimeOutOfAdherence," +
				"		ShiftHasEnded," +
				"		[State]" +
				"	) VALUES (" +
				"		:PersonId," +
				"		:Date," +
				"		:ToVersion," +
				"		:LastTimestamp," +
				"		:IsLastTimeInAdherence," +
				"		:TimeInAdherence," +
				"		:TimeOutOfAdherence," +
				"		:ShiftHasEnded," +
				"		:State" +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		[Version] = :ToVersion," +
				"		LastTimestamp = :LastTimestamp," +
				"		IsLastTimeInAdherence = :IsLastTimeInAdherence," +
				"		TimeInAdherence = :TimeInAdherence," +
				"		TimeOutOfAdherence = :TimeOutOfAdherence," +
				"		ShiftHasEnded = :ShiftHasEnded, " +
				"		[State] = :State " +
				";")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.Date)
				.SetParameter("FromVersion", fromVersion)
				.SetParameter("ToVersion", toVersion)
				.SetParameter("LastTimestamp", model.LastTimestamp)
				.SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
				.SetParameter("TimeInAdherence", model.TimeInAdherence)
				.SetParameter("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ShiftHasEnded", model.ShiftHasEnded)
				.SetParameter("State", _serializer.SerializeObject(model.State), NHibernateUtil.StringClob)
				.ExecuteUpdate();
			model.Version = toVersion;
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
				"	[State] AS StateJson, " +
				"	[Version]" +
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
				.AddScalar("Version", NHibernateUtil.Int32)
				.SetGuid("PersonId", personId)
				.SetDateOnly("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof (getModel)))
				.List<getModel>()
				.SingleOrDefault();

			if (result == null) return null;
			if (result.StateJson == null) return result;

			result.State = _deserializer.DeserializeObject<AdherencePercentageReadModelState[]>(result.StateJson);
			result.StateJson = null;

			return result;
		}

		public void Delete(Guid personId)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"DELETE ReadModel.AdherencePercentage " +
				"WHERE PersonId =:PersonId")
				.SetGuid("PersonId", personId)
				.ExecuteUpdate();
		}

		public bool HasData()
		{
			return (int)_unitOfWork.Current().CreateSqlQuery("SELECT count(*) FROM ReadModel.AdherencePercentage ").UniqueResult() > 0;
		}


		private class getModel : AdherencePercentageReadModel
		{
			public string StateJson { get; set; }
		}

		public AdherencePercentageReadModel Read(DateOnly date, Guid personId)
		{
			return _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date," +
				"	LastTimestamp," +
				"	IsLastTimeInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ShiftHasEnded " +
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
				.SetGuid("PersonId", personId)
				.SetDateOnly("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AdherencePercentageReadModel)))
				.List<AdherencePercentageReadModel>()
				.SingleOrDefault();
		}
	}
}