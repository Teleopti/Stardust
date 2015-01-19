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
				"	Model" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:Model" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model))
				.ExecuteUpdate();
		}

		public void Update(AdherenceDetailsReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherenceDetails SET" +
				"			Model = :Model " +
				"WHERE " +
				"	PersonId = :PersonId AND " +
				"	BelongsToDate =:Date")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.BelongsToDate)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model))
				.ExecuteUpdate();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date, " +
				"	Model " +
				"FROM ReadModel.AdherenceDetails WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("Model", NHibernateUtil.String)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List<internalModel>()
				.FirstOrDefault();

			if (result == null) return null;

			return new AdherenceDetailsReadModel
			{
				PersonId = result.PersonId,
				Date = result.Date,
				Model = _jsonDeserializer.DeserializeObject<AdherenceDetailsModel>(result.Model)
			};
		}

		public void ClearDetails(AdherenceDetailsReadModel model)
		{
			model.Model.Details.Clear();
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherenceDetails SET" +
				"			Model = :Model " +
				"WHERE " +
				"	PersonId = :PersonId AND " +
				"	BelongsToDate =:Date")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.BelongsToDate)
				.SetParameter("Model", _jsonSerializer.SerializeObject(model.Model))
				.ExecuteUpdate();
		}

		public bool HasData()
		{
			return (int)_unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.AdherenceDetails ").UniqueResult() > 0;
		}

		private class internalModel
		{
			public Guid PersonId { get; set; }
			public DateTime Date { get; set; }
			public string Model { get; set; }
		}
	}
}