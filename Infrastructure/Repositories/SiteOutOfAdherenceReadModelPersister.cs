using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SiteOutOfAdherenceReadModelPersister : ISiteOutOfAdherenceReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public SiteOutOfAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Persist(SiteOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE ReadModel.SiteOutOfAdherence AS T " +
				"USING (VALUES(:SiteId)) AS S (SiteId) " +
				"ON T.SiteId = S.SiteId " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		SiteId," +
				"		BusinessUnitId," +
				"		Count," +
				"		[State]" +
				"	) VALUES (" +
				"		:SiteId," +
				"		:BusinessUnitId," +
				"		:Count," +
				"		:State" +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		BusinessUnitId = :BusinessUnitId," +
				"		Count = :Count," +
				"		[State] = :State " +
				";")
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("Count", model.Count)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.ExecuteUpdate();
		}

		public SiteOutOfAdherenceReadModel Get(Guid siteId)
		{
			var result = _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"SiteId," +
					"BusinessUnitId," +
					"Count," +
					"[State] AS StateJson " +
					"FROM ReadModel.SiteOutOfAdherence " +
					"WHERE SiteId =:SiteId")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>()
				.SingleOrDefault();
			if (result == null) return null;
			if (result.StateJson == null) return result;

			result.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(result.StateJson);
			result.StateJson = null;
			return result;
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> GetForBusinessUnit(Guid businessUnitId)
		{
			var models = _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"SiteId," +
					"BusinessUnitId," +
					"Count," +
					"[State] AS StateJson " +
					"FROM ReadModel.SiteOutOfAdherence " + 
					"WHERE BusinessUnitId =:BusinessUnitId")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("BusinessUnitId", businessUnitId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>();

			models.ForEach(model =>
			{
				model.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(model.StateJson);
				model.StateJson = null;
			});
			return models;
		}

		public void Clear()
		{
			_unitOfWork.Current().CreateSqlQuery("DELETE FROM ReadModel.SiteOutOfAdherence ").ExecuteUpdate();
		}

		public bool HasData()
		{
			return (int) _unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.SiteOutOfAdherence ").UniqueResult() > 0;
		}

		private class internalModel : SiteOutOfAdherenceReadModel
		{
			public string StateJson { get; set; }
		} 
	}

	
}