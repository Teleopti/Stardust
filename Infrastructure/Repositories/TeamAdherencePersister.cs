using System;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TeamAdherencePersister : ITeamAdherencePersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public TeamAdherencePersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(TeamAdherenceReadModel model)
		{
			var existingReadModel = getModel(model.TeamId);
			if (existingReadModel == null)
			{
				saveReadModel(model);
			}
			else
			{
				updateReadModel(model);
			}
		}

		public TeamAdherenceReadModel Get(Guid teamId)
		{
			var ret = getModel(teamId);
			return ret ?? new TeamAdherenceReadModel(){TeamId = teamId};
		}

		private TeamAdherenceReadModel getModel(Guid teamId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	TeamId," +
				"	AgentsOutOfAdherence " +
				"FROM ReadModel.TeamAdherence WHERE" +
				"	TeamId =:TeamId ")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("AgentsOutOfAdherence", NHibernateUtil.Int16)
				.SetGuid("TeamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (TeamAdherenceReadModel))).List();
			return (TeamAdherenceReadModel)result.FirstOrNull();
		}

		private void updateReadModel(TeamAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.TeamAdherence SET" +
				"			AgentsOutOfAdherence = :AgentsOutOfAdherence " +
				"WHERE " +
				"	TeamId = :TeamId")
				.SetGuid("TeamId", model.TeamId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}

		private void saveReadModel(TeamAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.TeamAdherence (TeamId, AgentsOutOfAdherence) VALUES (:TeamId, :AgentsOutOfAdherence)")
				.SetGuid("TeamId", model.TeamId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}
	}
}