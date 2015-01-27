using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TeamOutOfAdherenceReadModelPersister : ITeamOutOfAdherenceReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public TeamOutOfAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(TeamOutOfAdherenceReadModel model)
		{
			var existingReadModel = getModel(model.TeamId);
			if (existingReadModel == null)
				saveReadModel(model);
			else
				updateReadModel(model);
		}

		public TeamOutOfAdherenceReadModel Get(Guid teamId)
		{
			return getModel(teamId);
		}

		public bool HasData()
		{
			return (int) _unitOfWork.Current() .CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.TeamOutOfAdherence ").UniqueResult() > 0;
		}

		public void Clear()
		{
			_unitOfWork.Current().CreateSqlQuery("DELETE FROM ReadModel.TeamOutOfAdherence ").ExecuteUpdate();
		}

		public IEnumerable<TeamOutOfAdherenceReadModel> GetForSite(Guid siteId)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM ReadModel.TeamOutOfAdherence WHERE SiteId =:SiteId")
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (TeamOutOfAdherenceReadModel)))
				.List()
				.Cast<TeamOutOfAdherenceReadModel>();
		}

		private TeamOutOfAdherenceReadModel getModel(Guid teamId)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM ReadModel.TeamOutOfAdherence WHERE TeamId =:TeamId")
				.SetParameter("TeamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (TeamOutOfAdherenceReadModel)))
				.List()
				.Cast<TeamOutOfAdherenceReadModel>()
				.SingleOrDefault();
		}

		private void updateReadModel(TeamOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.TeamOutOfAdherence SET Count = :Count, PersonIds = :PersonIds, SiteId = :SiteId WHERE TeamId = :TeamId")
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("PersonIds", model.PersonIds, NHibernateUtil.StringClob)
				.SetParameter("Count", model.Count)
				.ExecuteUpdate();
		}

		private void saveReadModel(TeamOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.TeamOutOfAdherence (SiteId, TeamId, Count, PersonIds) VALUES (:SiteId, :TeamId, :Count, :PersonIds)")
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("PersonIds", model.PersonIds, NHibernateUtil.StringClob)
				.SetParameter("Count", model.Count)
				.ExecuteUpdate();
		}
	}
}