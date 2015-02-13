using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
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

		public void Add(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherenceDetails " +
				"(" +
				"	PersonId," +
				"	BelongsToDate," +
				"	Model, " +
				"	ShiftEndTime," +
				"	[State]" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:Model," +
				"	:ShiftEndTime," +
				"	:State" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetDateTime("ShiftEndTime", model.ShiftEndTime)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model), NHibernateUtil.StringClob)
				.SetParameter("State", _jsonSerializer.SerializeObject(model.State), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherenceDetails SET" +
				"	Model = :Model, " +
				"	State = :State " +
				"WHERE " +
				"	PersonId = :PersonId AND " +
				"	BelongsToDate =:Date")
				"	ShiftEndTime =:ShiftEndTime")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.BelongsToDate)
				.SetDateTime("ShiftEndTime", model.ShiftEndTime)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model), NHibernateUtil.StringClob)
				.SetParameter("State", _jsonSerializer.SerializeObject(model.State), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date, " +
				"	Model AS ModelJson, " +
				"	ShiftEndTime, " +
				"	[State] AS StateJson " +
				"FROM ReadModel.AdherenceDetails WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("ModelJson", NHibernateUtil.String)
				.AddScalar("ShiftEndTime", NHibernateUtil.DateTime)
				.AddScalar("StateJson", NHibernateUtil.String)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
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
	}
}