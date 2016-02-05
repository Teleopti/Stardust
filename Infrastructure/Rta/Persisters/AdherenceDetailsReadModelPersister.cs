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
	public class AdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister, IAdherenceDetailsReadModelReader
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly IJsonDeserializer _jsonDeserializer;

		public AdherenceDetailsReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer jsonSerializer, IJsonDeserializer jsonDeserializer)
		{
			_unitOfWork = unitOfWork;
			_jsonSerializer = jsonSerializer;
			_jsonDeserializer = jsonDeserializer;
		}
		
		public void Persist(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE INTO ReadModel.AdherenceDetails AS T " +
				"USING (VALUES (:PersonId, :Date)) AS S (PersonId, Date) " +
				"ON T.PersonId = S.PersonId AND T.BelongsToDate = S.Date " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		PersonId," +
				"		BelongsToDate," +
				"		Model, " +
				"		[State]" +
				"	) VALUES (" +
				"		:PersonId," +
				"		:Date," +
				"		:Model," +
				"		:State" +
				"	)" +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		Model = :Model, " +
				"		[State] = :State" +
				";")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.Date)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model), NHibernateUtil.StringClob)
				.SetParameter("State", _jsonSerializer.SerializeObject(model.State), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public void Delete(Guid personId)
		{
			_unitOfWork.Current().CreateSqlQuery(
				   "DELETE ReadModel.AdherenceDetails " +
				   "WHERE " +
				   "	PersonId = :PersonId")
				   .SetGuid("PersonId", personId)
				   .ExecuteUpdate();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date, " +
				"	Model AS ModelJson, " +
				"	[State] AS StateJson " +
				"FROM ReadModel.AdherenceDetails WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("ModelJson", NHibernateUtil.String)
				.AddScalar("StateJson", NHibernateUtil.String)
				.SetGuid("PersonId", personId)
				.SetDateOnly("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List<internalModel>()
				.FirstOrDefault();

			if (result == null) return null;

			result.Model = _jsonDeserializer.DeserializeObject<AdherenceDetailsModel>(result.ModelJson);
			result.ModelJson = null;
			result.State = _jsonDeserializer.DeserializeObject<AdherenceDetailsReadModelState>(result.StateJson);
			result.StateJson = null;

			return result;
		}


		public bool HasData()
		{
			return (int)_unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.AdherenceDetails ").UniqueResult() > 0;
		}


		private class internalModel : AdherenceDetailsReadModel
		{
			public string ModelJson { get; set; }
			public string StateJson { get; set; }
		}

		public AdherenceDetailsReadModel Read(Guid personId, DateOnly date)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date, " +
				"	Model AS ModelJson " +
				"FROM ReadModel.AdherenceDetails WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("ModelJson", NHibernateUtil.String)
				.SetGuid("PersonId", personId)
				.SetDateOnly("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List<internalModel>()
				.FirstOrDefault();

			if (result == null) return null;

			result.Model = _jsonDeserializer.DeserializeObject<AdherenceDetailsModel>(result.ModelJson);
			result.ModelJson = null;

			return result;
		}
	}
}