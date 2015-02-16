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
			var existingReadModel = getModel(model.SiteId);
			if (existingReadModel == null)
				saveReadModel(model);
			else
				updateReadModel(model);
		}

		public SiteOutOfAdherenceReadModel Get(Guid siteId)
		{
			return getModel(siteId);
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> GetForBusinessUnit(Guid businessUnitId)
		{
			var models =  _unitOfWork.Current()
				.CreateSqlQuery("SELECT " + 
						"SiteId" +
						",BusinessUnitId" +
						",Count" + 
						",[State] AS StateJson " + 
						"FROM ReadModel.SiteOutOfAdherence WHERE BusinessUnitId =:BusinessUnitId")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("BusinessUnitId", businessUnitId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List()
				.Cast<internalModel>();

			if (!models.Any()) return new List<SiteOutOfAdherenceReadModel>(); ;
			models.ForEach(model =>
			{
				model.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(model.StateJson);
				model.StateJson = null;
			});
			return models;
		}

		private SiteOutOfAdherenceReadModel getModel(Guid siteId)
		{
			var result =  _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " + 
						"SiteId" +
						",BusinessUnitId" +
						",Count" + 
						",[State] AS StateJson" +
					" FROM ReadModel.SiteOutOfAdherence " +
				                "WHERE SiteId =:SiteId")
				.AddScalar("SiteId",NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List()
				.Cast<internalModel>()
				.SingleOrDefault();
			if (result == null) return null;
			if (result.StateJson == null) return result;
			
			result.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(result.StateJson);
			result.StateJson = null;
			return result;
			
		}

		public void Clear()
		{
			_unitOfWork.Current().CreateSqlQuery("DELETE FROM ReadModel.SiteOutOfAdherence ").ExecuteUpdate();
		}

		public bool HasData()
		{
			return (int) _unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.SiteOutOfAdherence ").UniqueResult() > 0;
		}

		private void updateReadModel(SiteOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.SiteOutOfAdherence SET Count = :Count, BusinessUnitId = :BusinessUnitId, [State] =:State WHERE SiteId = :SiteId")
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("Count", model.Count)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.ExecuteUpdate();
		}

		private void saveReadModel(SiteOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("INSERT INTO ReadModel.SiteOutOfAdherence (BusinessUnitId, SiteId, Count, [State]) VALUES (:BusinessUnitId, :SiteId, :Count, :State)")
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("Count", model.Count)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.ExecuteUpdate();
		}

		private class internalModel : SiteOutOfAdherenceReadModel
		{
			public string StateJson { get; set; }
		} 
	}

	
}