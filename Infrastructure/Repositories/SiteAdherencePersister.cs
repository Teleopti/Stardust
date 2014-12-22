using System;
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

		public SiteAdherenceReadModel Get(Guid teamId)
		{
			var ret = getModel(teamId);
			return ret ?? new SiteAdherenceReadModel() { SiteId = teamId };
		}

		private SiteAdherenceReadModel getModel(Guid teamId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	SiteId," +
				"	AgentsOutOfAdherence " +
				"FROM ReadModel.SiteAdherence WHERE" +
				"	SiteId =:SiteId ")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("AgentsOutOfAdherence", NHibernateUtil.Int16)
				.SetGuid("SiteId", teamId)
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
				"INSERT INTO ReadModel.SiteAdherence (SiteId, AgentsOutOfAdherence) VALUES (:SiteId, :AgentsOutOfAdherence)")
				.SetGuid("SiteId", model.SiteId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}
	}
}