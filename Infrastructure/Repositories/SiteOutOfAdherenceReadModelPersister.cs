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
	public class SiteOutOfAdherenceReadModelPersister : ISiteOutOfAdherenceReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public SiteOutOfAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
			var result = _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM ReadModel.SiteOutOfAdherence WHERE BusinessUnitId =:BusinessUnitId")
				.SetParameter("BusinessUnitId", businessUnitId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteOutOfAdherenceReadModel))).List();
			return result.Cast<SiteOutOfAdherenceReadModel>();
		}

		private SiteOutOfAdherenceReadModel getModel(Guid siteId)
		{
			var result = _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM ReadModel.SiteOutOfAdherence WHERE SiteId =:SiteId")
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteOutOfAdherenceReadModel))).List();
			return (SiteOutOfAdherenceReadModel)result.FirstOrNull();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool HasData()
		{
			var result = (int)_unitOfWork.Current()
				.CreateSqlQuery("SELECT count(*) FROM ReadModel.SiteOutOfAdherence ").UniqueResult();
			return result > 0;
		}

		private void updateReadModel(SiteOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.SiteOutOfAdherence SET Count = :Count, BusinessUnitId = :BusinessUnitId, PersonIds = :PersonIds WHERE SiteId = :SiteId")
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("Count", model.Count)
				.SetParameter("PersonIds", model.PersonIds )
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.ExecuteUpdate();
		}

		private void saveReadModel(SiteOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("INSERT INTO ReadModel.SiteOutOfAdherence (BusinessUnitId, SiteId, Count, PersonIds) VALUES (:BusinessUnitId, :SiteId, :Count, :PersonIds)")
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("Count", model.Count)
				.SetParameter("PersonIds", model.PersonIds )
				.ExecuteUpdate();
		}
	}
}