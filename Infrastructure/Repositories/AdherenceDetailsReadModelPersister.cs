using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Foundation;
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
				"	Model" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:Model" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("Model", new NewtonsoftJsonSerializer().SerializeObject(model.Model))
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
				.SetParameter("Model", new NewtonsoftJsonSerializer().SerializeObject(model.Model))
				.ExecuteUpdate();
		}

		public AdherenceDetailsReadModel Get(Guid personId, DateOnly date)
		{
			var readModel = new AdherenceDetailsReadModel();
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
				.SetReadOnly(true)
				.SetResultTransformer(Transformers.AliasToBean(typeof(adherenceDetailsReadModel)))
				.List<adherenceDetailsReadModel>().FirstOrDefault();
			if (result == null) return null;
			readModel.PersonId = result.PersonId;
			readModel.Date = result.Date;
			readModel.Model = new NewtonsoftJsonDeserializer().DeserializeObject<AdherenceDetailsModel>(result.Model);
			return readModel;
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
			.SetParameter("Model", new NewtonsoftJsonSerializer().SerializeObject(model.Model))
			.ExecuteUpdate();
		}

		public bool HasData()
		{
			return false;
		}

		class adherenceDetailsReadModel
		{
			public Guid PersonId { get; set; }
			public DateTime Date { get; set; }
			public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
			public string Model { get; set; }
		}
	}
}