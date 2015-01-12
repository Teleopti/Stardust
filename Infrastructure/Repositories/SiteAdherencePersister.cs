using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SiteAdherencePersister : ISiteAdherencePersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public SiteAdherencePersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(SiteAdherenceReadModel model)
		{
			var existingReadModel = getModel(model.SiteId);
			if (existingReadModel == null)
			{
				saveReadModel(model);
			}
			else
			{
				updateReadModel(model);
			}
		}

		public SiteAdherenceReadModel Get(Guid siteId)
		{
			var ret = getModel(siteId);
			return ret;
		}

		public IEnumerable<SiteAdherenceReadModel> GetAll(Guid businessUnitId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
			"SELECT " +
			"	SiteId," +
			"	AgentsOutOfAdherence " +
			"FROM ReadModel.SiteAdherence WHERE" +
			"	BusinessUnitId =:BusinessUnitId ")
			.AddScalar("SiteId", NHibernateUtil.Guid)
			.AddScalar("AgentsOutOfAdherence", NHibernateUtil.Int16)
			.SetGuid("BusinessUnitId", businessUnitId)
			.SetResultTransformer(Transformers.AliasToBean(typeof(SiteAdherenceReadModel))).List();
			return result.Cast<SiteAdherenceReadModel>();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool HasData()
		{
			throw new NotImplementedException();
		}


		private SiteAdherenceReadModel getModel(Guid siteId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	SiteId," +
				"	AgentsOutOfAdherence " +
				"FROM ReadModel.SiteAdherence WHERE" +
				"	SiteId =:SiteId ")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("AgentsOutOfAdherence", NHibernateUtil.Int16)
				.SetGuid("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteAdherenceReadModel))).List();
			return (SiteAdherenceReadModel)result.FirstOrNull();
		}

		private void updateReadModel(SiteAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.SiteAdherence SET" +
				"			AgentsOutOfAdherence = :AgentsOutOfAdherence " +
				"WHERE " +
				"	SiteId = :SiteId")
				.SetGuid("SiteId", model.SiteId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}

		private void saveReadModel(SiteAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.SiteAdherence (BusinessUnitId, SiteId, AgentsOutOfAdherence) VALUES (:BusinessUnitId, :SiteId, :AgentsOutOfAdherence)")
				.SetGuid("BusinessUnitId", model.BusinessUnitId)
				.SetGuid("SiteId", model.SiteId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}
	}
}